using WageringFeed.API.Infrastructure.Feed;
using WageringFeed.API.Infrastructure.Feed.Handlers;
using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Tests.Handlers;

public class EndOfFeedMessageHandlerTests
{
    [Fact]
    public async Task HandleAsync_EndOfFeedMessageShouldStop()
    {
        var handler = new EndOfFeedMessageHandler();
        var message = new BaseMessage { Type = MessageType.EndOfFeed, Payload = default, Timestamp = default };

        var action = await handler.HandleAsync(message, CancellationToken.None);

        Assert.Equal(WageringFeedHandlerAction.Stop, action);
    }
}
