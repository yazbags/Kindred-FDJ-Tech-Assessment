using WageringFeed.API.Application.Interfaces;
using WageringFeed.API.Application.Models;

namespace WageringFeed.API.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerStore _customerStore;

    public CustomerService(ICustomerStore customerStore)
    {
        _customerStore = customerStore;
    }

    public async Task<CustomerStatsResponse?> GetCustomerStats(int customerId)
    {
        // CustomerStore is not async, just wrapping it in a Task.Run to avoid blocking the thread from the controller
        var stats = await Task.Run(() => _customerStore.GetCustomerStats(customerId));
        if (stats is null)
        {
            return null;
        }

        return new CustomerStatsResponse
        {
            CustomerId = stats.CustomerId,
            Name = stats.Name,
            TotalStandToWin = decimal.Round(stats.TotalStandToWin, 2, MidpointRounding.AwayFromZero)
        };
    }

    public async Task<IReadOnlyList<CustomerDetails>> GetCustomers()
    {
        // CustomerStore is not async, just wrapping it in a Task.Run to avoid blocking the thread from the controller
        var stats = await Task.Run(() => _customerStore.GetCustomers());
        return stats.Select(c => new CustomerDetails { CustomerId = c.Key, Name = c.Value }).ToList();
    }
}
