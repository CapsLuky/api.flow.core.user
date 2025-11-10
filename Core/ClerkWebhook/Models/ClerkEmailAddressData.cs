using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.ClerkWebhook.Models;

[BsonIgnoreExtraElements]
public record ClerkEmailAddressData
{
    [JsonPropertyName("id")]
    [BsonElement("id")]
    public string? Id { get; set; }

    [JsonPropertyName("email_address")]
    [BsonElement("email_address")]
    public string? EmailAddress { get; set; }

    [JsonPropertyName("verification")]
    [BsonElement("verification")]
    public ClerkVerificationData? Verification { get; set; }

    [JsonPropertyName("created_at")]
    [BsonElement("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    [BsonElement("updated_at")]
    public long UpdatedAt { get; set; }
}