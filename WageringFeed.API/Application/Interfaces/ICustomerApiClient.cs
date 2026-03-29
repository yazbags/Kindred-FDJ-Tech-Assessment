using WageringFeed.API.Application.Models;

namespace WageringFeed.API.Application.Interfaces;

public interface ICustomerApiClient
{
    Task<CustomerDetails?> GetCustomerAsync(int customerId, CancellationToken cancellationToken);
}
