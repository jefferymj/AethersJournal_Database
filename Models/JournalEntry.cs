using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class JournalEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("journalentryid")]
    public string JournalEntryId { get; set; } // Unique ID for each journal entry

    [BsonElement("userid")]
    public string UserId { get; set; } // Associated user ID for this journal entry

    [BsonElement("title")]
    public string Title { get; set; }

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("content")]
    public string Content { get; set; }

    [BsonElement("summary")]
    public string Summary { get; set; }

    [BsonElement("chatid")]
    public string ChatId { get; set; }
}
