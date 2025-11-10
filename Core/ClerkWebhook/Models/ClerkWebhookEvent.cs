using System.Text.Json;
using System.Text.Json.Serialization;

namespace Core.ClerkWebhook.Models;

public record ClerkWebhookEvent
{
    [JsonPropertyName("data")]
    public JsonElement? Data { get; set; }
    
    [JsonPropertyName("object")]
    public string? Object { get; set; }
    
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    
    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
    
    [JsonPropertyName("instance_id")]
    public string? InstanceId { get; set; }
}