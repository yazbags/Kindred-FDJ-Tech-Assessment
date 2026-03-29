using System.Text.Json;
using Microsoft.Extensions.Options;
using WageringFeed.API.Application.Interfaces;
using WageringFeed.API.Application.Configuration;
using WageringFeed.API.Application.Models;

namespace WageringFeed.API.Infrastructure.CustomerApi;

public class CustomerApiClient : ICustomerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly WageringFeedOptions _options;
    private readonly ILogger<CustomerApiClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CustomerApiClient(
        HttpClient httpClient,
        IOptions<WageringFeedOptions> options,
        JsonSerializerOptions jsonOptions,
        ILogger<CustomerApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _jsonOptions = jsonOptions;
        _logger = logger;
    }

    public async Task<CustomerDetails?> GetCustomerAsync(int customerId, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"{_options.CustomerApiBaseUrl}/customer?customerId={customerId}&candidateId={_options.CandidateId}";
            var customer = await _httpClient.GetFromJsonAsync<CustomerDetails>(url, _jsonOptions, cancellationToken);
            return customer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch customer details for CustomerId={CustomerId}", customerId);
            return null;
        }
    }
}
