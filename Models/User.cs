using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id
    {
        get => _backingId;
        set
        {
            _backingId = value;
            UserId = value;
        }
    }

    private string? _backingId;

    [BsonElement("userid")]
    public string? UserId { get; private set; }

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

public class UserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
}