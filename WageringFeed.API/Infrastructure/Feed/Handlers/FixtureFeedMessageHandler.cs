using WageringFeed.API.Infrastructure.Feed;
using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Infrastructure.Feed.Handlers;

public class FixtureFeedMessageHandler : IWageringFeedMessageHandler
{
    public MessageType MessageType => MessageType.Fixture;

    public Task<WageringFeedHandlerAction> HandleAsync(BaseMessage message, CancellationToken cancellationToken)
    {
        // Ignoring for now, as we don't have any use for fixture messages at the moment
        return Task.FromResult(WageringFeedHandlerAction.Continue);
    }
}
