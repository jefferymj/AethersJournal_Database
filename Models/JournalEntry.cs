using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class JournalEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonElement("journalentryid")]
    public required string JournalEntryId { get; set; } // Unique ID for each journal entry

    [BsonElement("userid")]
    public required string UserId { get; set; } // Associated user ID for this journal entry

    [BsonElement("title")]
    public required string Title { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("content")]
    public required string Content { get; set; }

    [BsonElement("summary")]
    public required string Summary { get; set; }

    [BsonElement("chatid")]
    public required string ChatId { get; set; }
}
