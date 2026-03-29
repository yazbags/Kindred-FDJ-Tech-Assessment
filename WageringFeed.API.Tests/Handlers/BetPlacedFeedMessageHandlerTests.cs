using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using WageringFeed.API.Application.Interfaces;
using WageringFeed.API.Application.Models;
using WageringFeed.API.Infrastructure.Feed;
using WageringFeed.API.Infrastructure.Feed.Handlers;
using WageringFeed.API.Infrastructure.Feed.Models;

namespace WageringFeed.API.Tests.Handlers;

public class BetPlacedFeedMessageHandlerTests
{
    private readonly Mock<ICustomerStore> _mockStore;
    private readonly Mock<ICustomerApiClient> _mockApiClient;
    private readonly BetPlacedFeedMessageHandler _handler;

    public BetPlacedFeedMessageHandlerTests()
    {
        _mockStore = new Mock<ICustomerStore>();
        _mockApiClient = new Mock<ICustomerApiClient>();
        _handler = new BetPlacedFeedMessageHandler(
            _mockStore.Object,
            _mockApiClient.Object,
            TestJson.Options,
            NullLogger<BetPlacedFeedMessageHandler>.Instance);
    }

    private static BaseMessage CreateBetMessage(BetPlacedPayload bet)
    {
        var payload = JsonSerializer.SerializeToElement(bet, TestJson.Options);
        return new BaseMessage
        {
            Type = MessageType.BetPlaced,
            Payload = payload,
            Timestamp = DateTime.UtcNow
        };
    }

    [Theory]
    [InlineData(0, 2, 2)]
    [InlineData(2, 0, 1)]
    [InlineData(2, 2, 0)]
    public async Task HandleAsync_InvalidBetProperties(int customerId, decimal stake, decimal odds)
    {
        var message = CreateBetMessage(new BetPlacedPayload { CustomerId = customerId, Stake = stake, Odds = odds });

        var action = await _handler.HandleAsync(message, CancellationToken.None);

        Assert.Equal(WageringFeedHandlerAction.Continue, action);
        _mockStore.Verify(s => s.AddBet(It.IsAny<int>(), It.IsAny<decimal>()), Times.Never);
        _mockStore.Verify(s => s.AddCustomer(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _mockApiClient.Verify(a => a.GetCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static TheoryData<decimal, decimal, decimal> StandToWinCases => new()
    {
        { 20m, 3m, 40m },
        { 2m, 2m, 2m },
        { 1m, 6.42m, 5.42m },
        { 30m, 5.67m, 140.1m },
        { 0.01m, 2m, 0.01m },
        { 100m, 1.001m, 0.1m },
        { 10m, 1.91m, 9.1m },
        { 7.77m, 2.25m, 9.7125m },
        { 3.33m, 4.444444m, 11.46999852m },
        { 9999.99m, 1.5m, 4999.995m }
    };

    [Theory]
    [MemberData(nameof(StandToWinCases))]
    public async Task HandleAsync_ValidBetAndStakeValuesCalculatedCorrectly(
        decimal stake,
        decimal odds,
        decimal expectedStandToWin)
    {
        const int customerId = 99;
        _mockStore.Setup(s => s.HasCustomer(customerId)).Returns(true);
        var message = CreateBetMessage(new BetPlacedPayload { CustomerId = customerId, Stake = stake, Odds = odds });

        await _handler.HandleAsync(message, CancellationToken.None);

        _mockApiClient.Verify(a => a.GetCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockStore.Verify(s => s.AddBet(customerId, expectedStandToWin), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NewCustomerShouldBeAddedToStore()
    {
        _mockStore.Setup(s => s.HasCustomer(11)).Returns(false);
        _mockApiClient.Setup(a => a.GetCustomerAsync(11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerDetails { CustomerId = 11, Name = "Fetched" });
        var message = CreateBetMessage(new BetPlacedPayload { CustomerId = 11, Stake = 5, Odds = 4 });

        await _handler.HandleAsync(message, CancellationToken.None);

        _mockStore.Verify(s => s.AddCustomer(11, "Fetched"), Times.Once);
        _mockStore.Verify(s => s.AddBet(11, 5m * 4m - 5m), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_NullCustomerApiShouldStillRecordBet()
    {
        _mockStore.Setup(s => s.HasCustomer(12)).Returns(false);
        _mockApiClient.Setup(a => a.GetCustomerAsync(12, It.IsAny<CancellationToken>())).ReturnsAsync((CustomerDetails?)null);
        var message = CreateBetMessage(new BetPlacedPayload { CustomerId = 12, Stake = 2, Odds = 2 });

        await _handler.HandleAsync(message, CancellationToken.None);

        _mockStore.Verify(s => s.AddCustomer(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        _mockStore.Verify(s => s.AddBet(12, 2m), Times.Once);
    }
}
