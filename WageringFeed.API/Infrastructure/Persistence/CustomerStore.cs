using System.Collections.Concurrent;
using WageringFeed.API.Application.Interfaces;
using WageringFeed.API.Application.Models;

namespace WageringFeed.API.Infrastructure.Persistence;

public class CustomerStore : ICustomerStore
{
    private readonly ConcurrentDictionary<int, decimal> _standToWinTotals = new();
    private readonly ConcurrentDictionary<int, string> _customers = new();

    public void AddBet(int customerId, decimal standToWin)
    {
        _standToWinTotals.AddOrUpdate(
            customerId,
            standToWin,
            (_, existing) => existing + standToWin);
    }

    public void AddCustomer(int customerId, string customerName)
    {
        _customers.TryAdd(customerId, customerName);
    }

    public bool HasCustomer(int customerId)
    {
        return _customers.ContainsKey(customerId);
    }

    public CustomerStats? GetCustomerStats(int customerId)
    {
        if (!_standToWinTotals.TryGetValue(customerId, out var totalStandToWin))
        {
            return null;
        }

        _customers.TryGetValue(customerId, out var customerName);

        return new CustomerStats
        {
            CustomerId = customerId,
            Name = customerName ?? string.Empty,
            TotalStandToWin = totalStandToWin
        };
    }

    public KeyValuePair<int, string>[] GetCustomers()
    {
        return _customers.ToArray();
    }

    public void Clear()
    {
        _standToWinTotals.Clear();
        _customers.Clear();
    }
}
