using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Infrastructure.Feed.Handlers;

public interface IWageringFeedMessageHandler{
    MessageType MessageType { get; }

    Task<WageringFeedHandlerAction> HandleAsync(BaseMessage message, CancellationToken cancellationToken);
}
