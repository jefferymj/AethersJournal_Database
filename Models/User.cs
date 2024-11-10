using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonElement("userid")]
    public required string UserId { get; set; }

    [BsonElement("firstname")]
    public required string FirstName { get; set; }

    [BsonElement("lastname")]
    public required string LastName { get; set; }

    [BsonElement("email")]
    public required string Email { get; set; }

    [BsonElement("password")]
    public required string Password { get; set; }

    [BsonElement("journalEntryId")]
    public List<string> JournalEntries { get; set; } = new List<string>();
}
