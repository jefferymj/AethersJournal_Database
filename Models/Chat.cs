using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }

    [BsonElement("chatid")]
    public required string ChatId { get; set; }

    [BsonElement("messages")]
    public required List<Message> Messages { get; set; }
}

public class Message
{
    [BsonElement("time")]
    public DateTime Time { get; set; } = DateTime.Now;

    [BsonElement("sender")]
    [BsonRepresentation(BsonType.ObjectId)]
    public required string Sender { get; set; }

    [BsonElement("message")]
    public required string Content { get; set; }
}
