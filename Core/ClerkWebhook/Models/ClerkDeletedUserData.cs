using System.Text.Json.Serialization;

namespace Core.ClerkWebhook.Models;

public record ClerkDeletedUserData
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
    
    [JsonPropertyName("deleted")]
    public bool? Deleted { get; set; }

}