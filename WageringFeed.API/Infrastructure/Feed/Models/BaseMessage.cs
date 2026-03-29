using System.Text.Json;

namespace WageringFeed.API.Infrastructure.Feed.Models;

public class BaseMessage
{
    public MessageType Type { get; set; }
    public JsonElement Payload { get; set; }
    public DateTime Timestamp { get; set; }
}
