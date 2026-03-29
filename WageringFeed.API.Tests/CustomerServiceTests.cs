using Moq;
using WageringFeed.API.Application.Models;
using WageringFeed.API.Application.Services;
using WageringFeed.API.Application.Interfaces;

namespace WageringFeed.API.Tests;

public class CustomerServiceTests
{
    private readonly Mock<ICustomerStore> _mockStore;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _mockStore = new Mock<ICustomerStore>();
        _service = new CustomerService(_mockStore.Object);
    }

    [Fact]
    public async Task GetCustomerStats_ReturnsNullWhenStoreReturnsNull()
    {
        _mockStore.Setup(s => s.GetCustomerStats(5)).Returns((CustomerStats?)null);

        var result = await _service.GetCustomerStats(5);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCustomerStats_ReturnsResponseMappedFromStoreAggregate()
    {
        _mockStore.Setup(s => s.GetCustomerStats(1)).Returns(new CustomerStats
        {
            CustomerId = 1,
            Name = "Test",
            TotalStandToWin = 12.5m
        });

        var result = await _service.GetCustomerStats(1);

        Assert.NotNull(result);
        Assert.Equal(1L, result!.CustomerId);
        Assert.Equal("Test", result.Name);
        Assert.Equal(12.5m, result.TotalStandToWin);
    }

    public static TheoryData<decimal, decimal> TotalStandToWinRoundingCases => new()
    {
        { 0m, 0m },
        { 12.5m, 12.5m },
        { 54.65m, 54.65m },
        { 1.23456789m, 1.23m },
        { 10.444m, 10.44m },
        { 10.445m, 10.45m },
        { 54.645m, 54.65m },
        { 12.555m, 12.56m },
        { 99.999m, 100.00m },
        { 100.004m, 100.00m },
        { 100.005m, 100.01m },
        { 0.001m, 0.00m },
        { 0.005m, 0.01m },
        { 123456789.12m, 123456789.12m },
    };

    [Theory]
    [MemberData(nameof(TotalStandToWinRoundingCases))]
    public async Task GetCustomerStats_RoundsTotalStandToWinToTwoDecimalPlacesAwayFromZero(
        decimal aggregateFromStore,
        decimal expectedInResponse)
    {
        _mockStore.Setup(s => s.GetCustomerStats(99)).Returns(new CustomerStats
        {
            CustomerId = 99,
            Name = "Round",
            TotalStandToWin = aggregateFromStore
        });

        var result = await _service.GetCustomerStats(99);

        Assert.NotNull(result);
        Assert.Equal(expectedInResponse, result!.TotalStandToWin);
        Assert.Equal(99, result.CustomerId);
        Assert.Equal("Round", result.Name);
    }

    [Fact]
    public async Task GetCustomers_ReturnsListMappedFromStoreCustomerEntries()
    {
        _mockStore.Setup(s => s.GetCustomers()).Returns([new KeyValuePair<int, string>(7, "Sam")]);

        var list = await _service.GetCustomers();

        var item = Assert.Single(list);
        Assert.Equal(7, item.CustomerId);
        Assert.Equal("Sam", item.Name);
    }
}
