using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserStatus
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonElement("userid")]
    public required string UserId { get; set; }

    [BsonElement("userSummary")]
    public required string UserSummary { get; set; }
}
