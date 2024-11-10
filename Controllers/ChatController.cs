using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public ChatController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    // Create a new chat
    [HttpPost("addChat")]
    public async Task<IActionResult> AddChat([FromBody] Chat chat)
    {
        await _mongoDBService.Chat.InsertOneAsync(chat);
        return Ok("Chat added successfully");
    }

    // Get chat by chatid
    [HttpGet("getChat/{chatid}")]
    public async Task<IActionResult> GetChat(string chatid)
    {
        var chat = await _mongoDBService.Chat.Find(c => c.ChatId == chatid).FirstOrDefaultAsync();
        return chat != null ? Ok(chat) : NotFound("Chat not found");
    }

    // Update chat messages by chatid (adding new message)
    [HttpPut("addMessage/{chatid}")]
    public async Task<IActionResult> AddMessage(string chatid, [FromBody] Message newMessage)
    {
        var filter = Builders<Chat>.Filter.Eq(c => c.ChatId, chatid);
        var update = Builders<Chat>.Update.Push(c => c.Messages, newMessage);

        var result = await _mongoDBService.Chat.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0 ? Ok("Message added") : NotFound("Chat not found");
    }

    // Delete chat by chatid
    [HttpDelete("deleteChat/{chatid}")]
    public async Task<IActionResult> DeleteChat(string chatid)
    {
        var result = await _mongoDBService.Chat.DeleteOneAsync(c => c.ChatId == chatid);
        return result.DeletedCount > 0 ? Ok("Chat deleted") : NotFound("Chat not found");
    }
}
