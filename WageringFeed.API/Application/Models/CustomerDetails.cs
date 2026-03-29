using System.Text.Json.Serialization;

namespace WageringFeed.API.Application.Models;

public class CustomerDetails
{
    [JsonPropertyName("id")]
    public int CustomerId { get; set; }

    [JsonPropertyName("customerName")]
    public string Name { get; set; } = string.Empty;
}
