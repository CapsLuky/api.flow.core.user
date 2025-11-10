using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public record ClerkVerificationData
{
    [JsonPropertyName("status")]
    [BsonElement("status")]
    public string? Status { get; set; }

    [JsonPropertyName("strategy")]
    [BsonElement("strategy")]
    public string? Strategy { get; set; }
}