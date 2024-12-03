using Microsoft.Extensions.Options;
using MongoDB.Driver;

public class MongoDBService
{
    private readonly IMongoDatabase _database;

    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        var client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        _database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    public IMongoCollection<UserStatus> UserStatus => _database.GetCollection<UserStatus>("userStatus");
    public IMongoCollection<JournalEntry> JournalEntries => _database.GetCollection<JournalEntry>("journalEntries");
    public IMongoCollection<Chat> Chat => _database.GetCollection<Chat>("chat");
    public IMongoCollection<Session> Session => _database.GetCollection<Session>("session");
}
