using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WageringFeed.API.Application.Configuration;
using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Infrastructure.Feed;

public class WageringFeedConsumer : BackgroundService
{
    private readonly WageringFeedMessageDispatcher _dispatcher;
    private readonly WageringFeedOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<WageringFeedConsumer> _logger;

    public WageringFeedConsumer(
        WageringFeedMessageDispatcher dispatcher,
        IOptions<WageringFeedOptions> options,
        JsonSerializerOptions jsonOptions,
        ILogger<WageringFeedConsumer> logger)
    {
        _dispatcher = dispatcher;
        _options = options.Value;
        _jsonOptions = jsonOptions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // With more time: extract the connect and receive/dispatch loop (e.g. WageringFeedMessageReceiver) and introduce
            // something like IWageringFeedWebSocket so can be unit tested with a fake socket without ClientWebSocket.
            using var webSocket = new ClientWebSocket();
            var uri = new Uri($"{_options.WebSocketBaseUrl}?candidateId={_options.CandidateId}");
            await webSocket.ConnectAsync(uri, stoppingToken);
            _logger.LogInformation("Connected to wagering feed");

            await ReceiveAsync(webSocket, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Wagering feed stopped with normal shutdown");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wagering feed stopped with error");
        }
    }

    private async Task ReceiveAsync(ClientWebSocket webSocket, CancellationToken cancellationToken)
    {
        var buffer = new byte[_options.BufferSize];

        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                break;
            }

            var baseMessageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

            try
            {
                var message = JsonSerializer.Deserialize<BaseMessage>(baseMessageJson, _jsonOptions);

                if (message is null)
                {
                    continue;
                }

                var resultAction = await _dispatcher.DispatchAsync(message.Type, message, cancellationToken);

                if (resultAction == WageringFeedHandlerAction.Stop)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "EndOfFeed", cancellationToken);
                    break;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Unable to deserialize BaseMessage. Skip and continue feed ingestion.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception handling BetPlaced message. Skip and continue feed ingestion.");
            }
        }
    }
}
