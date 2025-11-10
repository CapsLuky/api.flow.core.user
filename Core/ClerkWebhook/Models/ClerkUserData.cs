using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.ClerkWebhook.Models;

[BsonIgnoreExtraElements]
public record ClerkUserData
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } // _id do MongoDB

    [JsonPropertyName("id")] // Nome no JSON do webhook
    [BsonElement("clerk_id")] // Nome do campo no MongoDB
    public string? ClerkId { get; set; }

    [JsonPropertyName("object")]
    [BsonElement("object")]
    public string? Object { get; set; }

    [JsonPropertyName("external_id")]
    [BsonElement("external_id")]
    public string? ExternalId { get; set; }

    [JsonPropertyName("first_name")]
    [BsonElement("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("last_name")]
    [BsonElement("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("username")]
    [BsonElement("username")]
    public string? Username { get; set; }

    [JsonPropertyName("banned")]
    [BsonElement("banned")]
    public bool Banned { get; set; }

    [JsonPropertyName("locked")]
    [BsonElement("locked")]
    public bool Locked { get; set; }

    [JsonPropertyName("image_url")]
    [BsonElement("image_url")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("has_image")]
    [BsonElement("has_image")]
    public bool HasImage { get; set; }

    [JsonPropertyName("profile_image_url")]
    [BsonElement("profile_image_url")]
    public string? ProfileImageUrl { get; set; }

    [JsonPropertyName("two_factor_enabled")]
    [BsonElement("two_factor_enabled")]
    public bool TwoFactorEnabled { get; set; }

    [JsonPropertyName("totp_enabled")]
    [BsonElement("totp_enabled")]
    public bool TotpEnabled { get; set; }

    [JsonPropertyName("backup_code_enabled")]
    [BsonElement("backup_code_enabled")]
    public bool BackupCodeEnabled { get; set; }

    [JsonPropertyName("password_enabled")]
    [BsonElement("password_enabled")]
    public bool PasswordEnabled { get; set; }

    [JsonPropertyName("primary_email_address_id")]
    [BsonElement("primary_email_address_id")]
    public string? PrimaryEmailAddressId { get; set; }

    [JsonPropertyName("primary_phone_number_id")]
    [BsonElement("primary_phone_number_id")]
    public string? PrimaryPhoneNumberId { get; set; }
    
    [JsonPropertyName("public_metadata")]
    [BsonElement("public_metadata")]
    [BsonRepresentation(BsonType.Document)]
    public Dictionary<string, object>? PublicMetadata { get; set; }

    [JsonPropertyName("private_metadata")]
    [BsonElement("private_metadata")]
    [BsonRepresentation(BsonType.Document)]
    public Dictionary<string, object>? PrivateMetadata { get; set; }

    [JsonPropertyName("unsafe_metadata")]
    [BsonElement("unsafe_metadata")]
    [BsonRepresentation(BsonType.Document)]
    public Dictionary<string, object>? UnsafeMetadata { get; set; }

    [JsonPropertyName("email_addresses")]
    [BsonElement("email_addresses")]
    public List<ClerkEmailAddressData>? EmailAddresses { get; set; }

    [JsonPropertyName("phone_numbers")]
    [BsonElement("phone_numbers")]
    public List<ClerkPhoneNumberData>? PhoneNumbers { get; set; }
    
    [JsonPropertyName("created_at")]
    [BsonElement("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    [BsonElement("updated_at")]
    public long UpdatedAt { get; set; }

    [JsonPropertyName("last_sign_in_at")]
    [BsonElement("last_sign_in_at")]
    public long? LastSignInAt { get; set; }
}