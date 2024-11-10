using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Chat
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("chatid")]
    public string ChatId { get; set; }

    [BsonElement("messages")]
    public List<Message> Messages { get; set; }
}

public class Message
{
    [BsonElement("time")]
    public DateTime Time { get; set; } = DateTime.Now;

    [BsonElement("sender")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Sender { get; set; }

    [BsonElement("message")]
    public string Content { get; set; }
}
