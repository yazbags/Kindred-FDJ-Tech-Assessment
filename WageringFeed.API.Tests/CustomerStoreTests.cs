using WageringFeed.API.Infrastructure.Persistence;

namespace WageringFeed.API.Tests;

public class CustomerStoreTests
{
    private readonly CustomerStore _store;

    public CustomerStoreTests()
    {
        _store = new CustomerStore();
    }

    [Fact]
    public void GetCustomerStats_ReturnsNullWhenCustomerHasNoBets()
    {
        Assert.Null(_store.GetCustomerStats(42));
    }

    [Fact]
    public void GetCustomerStats_ReturnsNullWhenCustomerOnlyRegisteredWithoutBets()
    {
        _store.AddCustomer(7, "OnlyRegistered");

        Assert.Null(_store.GetCustomerStats(7));
    }

    [Fact]
    public void GetCustomerStats_ReturnsTotalAndEmptyNameWhenBetsExistWithoutRegisteredCustomer()
    {
        _store.AddBet(1, 10m);

        var stats = _store.GetCustomerStats(1);

        Assert.NotNull(stats);
        Assert.Equal(1, stats!.CustomerId);
        Assert.Equal(string.Empty, stats.Name);
        Assert.Equal(10m, stats.TotalStandToWin);
    }

    [Fact]
    public void GetCustomerStats_IncludesRegisteredNameWhenCustomerWasAddedBeforeBets()
    {
        _store.AddCustomer(2, "Ada");
        _store.AddBet(2, 5m);

        var stats = _store.GetCustomerStats(2);

        Assert.NotNull(stats);
        Assert.Equal("Ada", stats!.Name);
        Assert.Equal(5m, stats.TotalStandToWin);
    }

    [Fact]
    public void GetCustomerStats_SumsStandToWinForMultipleAddBetCallsOnSameCustomer()
    {
        _store.AddBet(3, 4m);
        _store.AddBet(3, 6m);

        var stats = _store.GetCustomerStats(3);

        Assert.Equal(10m, stats!.TotalStandToWin);
    }

    public static TheoryData<decimal, decimal, decimal> TwoAddBetDecimalTotals => new()
    {
        { 4m, 6m, 10m },
        { 0.01m, 0.02m, 0.03m },
        { 1m, 6.42m, 7.42m },
        { 30m, 5.67m, 35.67m },
        { 7.77m, 2.25m, 10.02m },
        { 999.99m, 0.005m, 999.995m },
        { 3.33m, 4.444444m, 7.774444m }
    };

    [Theory]
    [MemberData(nameof(TwoAddBetDecimalTotals))]
    public void GetCustomerStats_SumsTwoDecimalStandToWinAmountsCorrectly(decimal first, decimal second, decimal expectedTotal)
    {
        const int customerId = 100;

        _store.AddBet(customerId, first);
        _store.AddBet(customerId, second);

        var stats = _store.GetCustomerStats(customerId);

        Assert.NotNull(stats);
        Assert.Equal(expectedTotal, stats!.TotalStandToWin);
    }

    public static TheoryData<decimal, decimal, decimal, decimal> ThreeAddBetDecimalTotals => new()
    {
        { 1m, 2m, 3m, 6m },
        { 10.5m, 0.1m, 2.4m, 13m },
        { 140.1m, 5.42m, 0.01m, 145.53m }
    };

    [Theory]
    [MemberData(nameof(ThreeAddBetDecimalTotals))]
    public void GetCustomerStats_SumsThreeDecimalStandToWinAmountsCorrectly(
        decimal a,
        decimal b,
        decimal c,
        decimal expectedTotal)
    {
        const int customerId = 101;

        _store.AddBet(customerId, a);
        _store.AddBet(customerId, b);
        _store.AddBet(customerId, c);

        var stats = _store.GetCustomerStats(customerId);

        Assert.NotNull(stats);
        Assert.Equal(expectedTotal, stats!.TotalStandToWin);
    }

    public static TheoryData<decimal> SingleStandToWinAmounts => new()
    {
        0m,
        0.01m,
        1m,
        6.42m,
        140.1m,
        4999.995m
    };

    [Theory]
    [MemberData(nameof(SingleStandToWinAmounts))]
    public void GetCustomerStats_ReturnsSameDecimalForSingleAddBet(decimal standToWin)
    {
        const int customerId = 102;

        _store.AddBet(customerId, standToWin);

        var stats = _store.GetCustomerStats(customerId);

        Assert.NotNull(stats);
        Assert.Equal(standToWin, stats!.TotalStandToWin);
    }

    [Fact]
    public void GetCustomerStats_IsNotNullWhenOnlyZeroStandToWinWasRecorded()
    {
        _store.AddBet(8, 0m);

        var stats = _store.GetCustomerStats(8);

        Assert.NotNull(stats);
        Assert.Equal(0m, stats!.TotalStandToWin);
    }

    [Fact]
    public void GetCustomerStats_SubtractsWhenNegativeStandToWinAdded()
    {
        const int customerId = 103;
        _store.AddBet(customerId, 100m);
        _store.AddBet(customerId, -25.5m);

        var stats = _store.GetCustomerStats(customerId);

        Assert.NotNull(stats);
        Assert.Equal(74.5m, stats!.TotalStandToWin);
    }

    [Fact]
    public void GetCustomerStats_AllNegativeAddsProduceNegativeTotal()
    {
        const int customerId = 104;
        _store.AddBet(customerId, -10m);
        _store.AddBet(customerId, -3.33m);

        var stats = _store.GetCustomerStats(customerId);

        Assert.NotNull(stats);
        Assert.Equal(-13.33m, stats!.TotalStandToWin);
    }

    [Fact]
    public void AddCustomer_SecondCallForSameIdDoesNotReplaceName()
    {
        _store.AddCustomer(5, "First");
        _store.AddCustomer(5, "SecondIgnored");

        Assert.True(_store.HasCustomer(5));
        _store.AddBet(5, 1m);
        var stats = _store.GetCustomerStats(5);
        Assert.Equal("First", stats!.Name);
    }

    [Fact]
    public void HasCustomer_ReturnsFalseUntilAddCustomerThenReturnsTrue()
    {
        Assert.False(_store.HasCustomer(9));

        _store.AddCustomer(9, "Bob");
        Assert.True(_store.HasCustomer(9));
    }

    [Fact]
    public void GetCustomers_ReturnsOnlyCustomersAddedViaAddCustomer()
    {
        _store.AddCustomer(1, "A");
        _store.AddBet(2, 1m);

        var customers = _store.GetCustomers();

        Assert.Single(customers);
        Assert.Equal(1, customers[0].Key);
        Assert.Equal("A", customers[0].Value);
    }

    [Fact]
    public void Clear_RemovesAllBetsAndRegisteredCustomers()
    {
        _store.AddCustomer(1, "A");
        _store.AddBet(1, 1m);

        _store.Clear();

        Assert.Null(_store.GetCustomerStats(1));
        Assert.Empty(_store.GetCustomers());
    }
}
