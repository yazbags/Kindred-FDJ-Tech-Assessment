using WageringFeed.API.Infrastructure.Feed;
using WageringFeed.API.Infrastructure.Feed.Handlers;
using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Tests.Handlers;

public class FixtureFeedMessageHandlerTests
{
    [Fact]
    public async Task HandleAsync_FixtureShouldBeIgnoredAndContinue()
    {
        var handler = new FixtureFeedMessageHandler();
        var message = new BaseMessage { Type = MessageType.Fixture, Payload = default, Timestamp = default };

        var action = await handler.HandleAsync(message, CancellationToken.None);

        Assert.Equal(WageringFeedHandlerAction.Continue, action);
    }
}
