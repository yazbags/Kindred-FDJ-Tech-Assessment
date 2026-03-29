using System.Text.Json;
using WageringFeed.API.Application.Interfaces;
using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Infrastructure.Feed.Handlers;

public class BetPlacedFeedMessageHandler : IWageringFeedMessageHandler
{
    private readonly ICustomerStore _customerStore;
    private readonly ICustomerApiClient _customerApiClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<BetPlacedFeedMessageHandler> _logger;

    public BetPlacedFeedMessageHandler(
        ICustomerStore customerStore,
        ICustomerApiClient customerApiClient,
        JsonSerializerOptions jsonOptions,
        ILogger<BetPlacedFeedMessageHandler> logger)
    {
        _customerStore = customerStore;
        _customerApiClient = customerApiClient;
        _jsonOptions = jsonOptions;
        _logger = logger;
    }

    public MessageType MessageType => MessageType.BetPlaced;

    public async Task<WageringFeedHandlerAction> HandleAsync(BaseMessage message, CancellationToken cancellationToken)
    {
        try
        {
            var bet = message.Payload.Deserialize<BetPlacedPayload>(_jsonOptions);

            if (bet is null || bet.CustomerId <= 0 || bet.Odds <= 0 || bet.Stake <= 0)
            {
                _logger.LogWarning("Incomplete bet data. Skip and continue feed ingestion.");
                return WageringFeedHandlerAction.Continue;
            }

            if (!_customerStore.HasCustomer(bet.CustomerId))
            {
                var customer = await _customerApiClient.GetCustomerAsync(bet.CustomerId, cancellationToken);
                if (customer is not null)
                    _customerStore.AddCustomer(bet.CustomerId, customer.Name);
            }

            var standToWin = bet.Stake * bet.Odds - bet.Stake;
            _customerStore.AddBet(bet.CustomerId, standToWin);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Unable to deserialize BetPlaced. Skip and continue feed ingestion.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception handling BetPlaced message. Skip and continue feed ingestion.");
        }

        return WageringFeedHandlerAction.Continue;
    }
}
