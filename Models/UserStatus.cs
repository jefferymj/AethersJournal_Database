using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserStatus
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userid")]
    public string UserId { get; set; }

    [BsonElement("userSummary")]
    public string UserSummary { get; set; }
}
