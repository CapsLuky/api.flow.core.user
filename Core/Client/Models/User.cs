using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Core.Client.Models;

[Obsolete("Use 'ClerkWebhook/Models/ClerkUserData' instead. This record will be removed in a future version.")]
public record User(
    [property: BsonId]
    [property: BsonRepresentation(BsonType.ObjectId)]
    string Id,

    [property: BsonElement("clerkId")]
    string ClerckId,

    [property: BsonElement("email")]
    string Email,

    [property: BsonElement("name")]
    string Name,

    [property: BsonElement("profileType")]
    string ProfileType,

    [property: BsonElement("role")]
    string Role,

    [property: BsonElement("createdAt")]
    DateTime CreatedAt,

    [property: BsonElement("updatedAt")]
    DateTime UpdatedAt
    );