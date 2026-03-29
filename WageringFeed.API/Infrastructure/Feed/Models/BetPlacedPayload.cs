namespace WageringFeed.API.Infrastructure.Feed.Models;

public class BetPlacedPayload
{
    // only mapping relevant properties for now
    public int CustomerId { get; set; }
    public decimal Stake { get; set; }
    public decimal Odds { get; set; }
}
