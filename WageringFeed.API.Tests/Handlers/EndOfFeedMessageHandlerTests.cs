using Moq;
using WageringFeed.API.Application.Interfaces;
using WageringFeed.API.Infrastructure.Feed;
using WageringFeed.API.Infrastructure.Feed.Handlers;
using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Tests.Handlers;

public class EndOfFeedMessageHandlerTests
{
    [Fact]
    public async Task HandleAsync_EndOfFeedMessageShouldStopAndClear()
    {
        var store = new Mock<ICustomerStore>();
        var handler = new EndOfFeedMessageHandler(store.Object);
        var message = new BaseMessage { Type = MessageType.EndOfFeed, Payload = default, Timestamp = default };

        var action = await handler.HandleAsync(message, CancellationToken.None);

        Assert.Equal(WageringFeedHandlerAction.Stop, action);
        store.Verify(s => s.Clear(), Times.Once);
    }
}
