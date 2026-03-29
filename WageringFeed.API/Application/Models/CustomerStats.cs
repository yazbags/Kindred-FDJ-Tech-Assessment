namespace WageringFeed.API.Application.Models;

public class CustomerStats
{
    public int CustomerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal TotalStandToWin { get; init; }
}
