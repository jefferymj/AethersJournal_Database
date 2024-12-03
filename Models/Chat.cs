using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? _id
    {
        get => _backingId;
        set
        {
            _backingId = value;
            ChatId = value;
        }
    }

    private string? _backingId;

    [BsonElement("chatid")]
    public string? ChatId { get; set; }

    [BsonElement("journal_id")]
    public string? JournalId { get; set; }

    [BsonElement("messages")]
    public List<Message> Messages { get; set; } = [];
}

public class Message
{
    [BsonElement("time")]
    public DateTime Time { get; set; } = DateTime.Now;

    [BsonElement("sender")]
    public required string Sender { get; set; }

    [BsonElement("message")]
    public required string Content { get; set; }
}
