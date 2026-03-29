namespace WageringFeed.API.Application.Models;

public class CustomerStatsResponse
{
    public int CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TotalStandToWin { get; set; }
}
