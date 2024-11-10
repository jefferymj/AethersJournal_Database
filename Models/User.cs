using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userid")]
    public string UserId { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("password")]
    public string Password { get; set; }

    [BsonElement("journalEntryId")]
    public List<string> JournalEntries { get; set; } = new List<string>();
}
