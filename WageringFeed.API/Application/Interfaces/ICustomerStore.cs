using WageringFeed.API.Application.Models;

namespace WageringFeed.API.Application.Interfaces;

public interface ICustomerStore
{
    void AddBet(int customerId, decimal standToWin);

    void AddCustomer(int customerId, string customerName);

    bool HasCustomer(int customerId);

    CustomerStats? GetCustomerStats(int customerId);

    KeyValuePair<int, string>[] GetCustomers();

    void Clear();
}
