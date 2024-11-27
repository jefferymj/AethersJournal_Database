using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class JournalEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("userid")]
    public string? UserId { get; set; } // Associated user ID for this journal entry

    [BsonElement("title")]
    public required string Title { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("content")]
    public required string Content { get; set; }

    [BsonElement("summary")]
    public required string Summary { get; set; }

    [BsonElement("chatid")]
    public string? ChatId { get; set; }
}

public class JournalEntryRequest
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Date { get; set; } // Consider using DateTime if you expect a proper date format
}