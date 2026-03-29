namespace WageringFeed.API.Application.Configuration;

public class WageringFeedOptions
{
    public const string SectionName = "WageringFeed";

    public string WebSocketBaseUrl { get; set; } = string.Empty;
    public string CustomerApiBaseUrl { get; set; } = string.Empty;
    public string CandidateId { get; set; } = string.Empty;
    public int BufferSize { get; set; } = 1024;
}
