using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Session
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } // Unique identifier for the session

    [BsonElement("token")]
    public required string SessionToken { get; set; } // Unique token for the session

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string UserId { get; set; } // Reference to the User ID
}
