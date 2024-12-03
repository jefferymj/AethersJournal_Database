using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;
    private readonly HttpClient _httpClient;

    public ChatController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
        _httpClient = new HttpClient();
    }

    // Create a new chat
    [HttpPost("addChat")]
    public async Task<IActionResult> AddChat([FromBody] Chat chat)
    {
        await _mongoDBService.Chat.InsertOneAsync(chat);
        return Ok("Chat added successfully");
    }

    // Get chat by chatid
    [HttpGet("getChat/{journalId}")]
    public async Task<IActionResult> GetChat(string journalId)
    {
        var chat = await _mongoDBService.Chat.Find(c => c.JournalId == journalId).FirstOrDefaultAsync();
        return chat != null ? Ok(chat) : NotFound("Chat not found");
    }

    // Update chat messages by chatid (adding new message)
    [HttpPost("addMessage/{journalId}")]
    public async Task<IActionResult> AddMessage(string journalId, [FromBody] Message newMessage)
    {
        FilterDefinition<Chat> filter = Builders<Chat>.Filter.Eq(c => c.JournalId, journalId);

        Chat chat = await _mongoDBService.Chat.Find(filter).FirstOrDefaultAsync();

        if (chat == null)
        {
            return NotFound("Chat not found for the given journal entry.");
        }

        JournalEntry journalEntry = await _mongoDBService.JournalEntries
            .Find(j => j.Id == journalId)
            .FirstOrDefaultAsync();

        if (journalEntry == null)
        {
            return NotFound("Journal Entry not found");
        }

        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append($"Context: {journalEntry.Summary}.");

        List<Message> previousMessageEntry = chat.Messages;

        foreach (Message message in previousMessageEntry)
        {
            string messageContent = message.Content;
            string sender = message.Sender;
            stringBuilder.Append($"{sender}: {messageContent}");
        }

        stringBuilder.Append($"user: {newMessage.Content}.");

        UserMessage userMessage = new()
        {
            Message = stringBuilder.ToString(),
            Context = journalEntry.Summary
        };

        ChatResponse chatResponse = await GetAIChatResponse(userMessage);

        Message newAIMesaage = new()
        {
            Sender = "AI",
            Content = chatResponse.response
        };

        var userMessageUpdate = Builders<Chat>.Update
            .Push(c => c.Messages, newMessage);

        var aiMessageUpdate = Builders<Chat>.Update
            .Push(c => c.Messages, newAIMesaage);

        var userMessageResult = await _mongoDBService.Chat.UpdateOneAsync(filter, userMessageUpdate);
        var aiMessageResult2 = await _mongoDBService.Chat.UpdateOneAsync(filter, aiMessageUpdate);
        return (userMessageResult.ModifiedCount > 0 && aiMessageResult2.ModifiedCount > 0) ? Ok(new { aiResponse = chatResponse.response }) : NotFound("Chat not found");
    }

    // Delete chat by chatid
    [HttpDelete("deleteChat/{journalId}")]
    public async Task<IActionResult> DeleteChat(string journalId)
    {
        var result = await _mongoDBService.Chat.DeleteOneAsync(c => c.JournalId == journalId);
        return result.DeletedCount > 0 ? Ok("Chat deleted") : NotFound("Chat not found");
    }
    public async Task<ChatResponse> GetAIChatResponse(UserMessage userMessage)
    {
        var jsonContent = JsonSerializer.Serialize(userMessage);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://aether-czdxepa8htg7eec5.canadacentral-01.azurewebsites.net/api/chat/send-message", httpContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        var summaryResponse = JsonSerializer.Deserialize<ChatResponse>(responseContent);
        return summaryResponse!;
    }
}
