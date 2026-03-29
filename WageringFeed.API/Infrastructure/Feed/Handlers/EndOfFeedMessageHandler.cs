using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Infrastructure.Feed.Handlers;

public class EndOfFeedMessageHandler : IWageringFeedMessageHandler
{
    public MessageType MessageType => MessageType.EndOfFeed;

    public Task<WageringFeedHandlerAction> HandleAsync(BaseMessage message, CancellationToken cancellationToken)
    {
        return Task.FromResult(WageringFeedHandlerAction.Stop);
    }
}
