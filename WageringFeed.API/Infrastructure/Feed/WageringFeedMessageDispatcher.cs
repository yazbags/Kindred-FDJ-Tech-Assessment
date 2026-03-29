using WageringFeed.API.Infrastructure.Feed.Handlers;
using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Infrastructure.Feed;

public class WageringFeedMessageDispatcher
{
    private readonly Dictionary<MessageType, IWageringFeedMessageHandler> _handlers;

    public WageringFeedMessageDispatcher(IEnumerable<IWageringFeedMessageHandler> handlers)
    {
        _handlers = handlers.ToDictionary(h => h.MessageType);
    }

    public async Task<WageringFeedHandlerAction> DispatchAsync(
        MessageType type,
        BaseMessage message,
        CancellationToken cancellationToken)
    {
        if (!_handlers.TryGetValue(type, out var handler))
        {
            // Message type we don't need to handle so continue
            return WageringFeedHandlerAction.Continue;
        }

        return await handler.HandleAsync(message, cancellationToken);
    }
}
