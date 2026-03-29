using WageringFeed.API.Application.Models;

namespace WageringFeed.API.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerStatsResponse?> GetCustomerStats(int customerId);

    Task<IReadOnlyList<CustomerDetails>> GetCustomers();
}
