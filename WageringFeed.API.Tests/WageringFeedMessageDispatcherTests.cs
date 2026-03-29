using Moq;
using WageringFeed.API.Infrastructure.Feed;
using WageringFeed.API.Infrastructure.Feed.Handlers;
using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Tests;

public class WageringFeedMessageDispatcherTests
{
    private readonly Mock<IWageringFeedMessageHandler> _mockHandler;

    public WageringFeedMessageDispatcherTests()
    {
        _mockHandler = new Mock<IWageringFeedMessageHandler>();
    }

    [Fact]
    public async Task DispatchAsync_ReturnsContinueWithoutInvokingHandlersWhenNoHandlerRegisteredForMessageType()
    {
        _mockHandler.SetupGet(h => h.MessageType).Returns(MessageType.BetPlaced);
        var dispatcher = new WageringFeedMessageDispatcher([_mockHandler.Object]);
        var message = new BaseMessage { Type = MessageType.Fixture, Payload = default, Timestamp = default };

        var action = await dispatcher.DispatchAsync(MessageType.Fixture, message, CancellationToken.None);

        Assert.Equal(WageringFeedHandlerAction.Continue, action);
        _mockHandler.Verify(
            h => h.HandleAsync(It.IsAny<BaseMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DispatchAsync_InvokesHandlerAndReturnsItsActionWhenMessageTypeMatches()
    {
        _mockHandler.SetupGet(h => h.MessageType).Returns(MessageType.EndOfFeed);
        _mockHandler
            .Setup(h => h.HandleAsync(It.IsAny<BaseMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(WageringFeedHandlerAction.Stop);
        var dispatcher = new WageringFeedMessageDispatcher([_mockHandler.Object]);
        var message = new BaseMessage { Type = MessageType.EndOfFeed, Payload = default, Timestamp = default };

        var action = await dispatcher.DispatchAsync(MessageType.EndOfFeed, message, CancellationToken.None);

        Assert.Equal(WageringFeedHandlerAction.Stop, action);
        _mockHandler.Verify(h => h.HandleAsync(message, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Constructor_ThrowsWhenTwoHandlersShareSameMessageType()
    {
        var a = new Mock<IWageringFeedMessageHandler>();
        a.SetupGet(h => h.MessageType).Returns(MessageType.BetPlaced);
        var b = new Mock<IWageringFeedMessageHandler>();
        b.SetupGet(h => h.MessageType).Returns(MessageType.BetPlaced);

        Assert.Throws<ArgumentException>(() => new WageringFeedMessageDispatcher([a.Object, b.Object]));
    }
}
