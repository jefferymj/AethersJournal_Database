using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase {
    private readonly MongoDBService _mongoDBService;

    public SessionController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    // Create a new session
    [HttpPost("addSession")]
    async public Task<IActionResult> addSession() {
        var session = new Session {
            Id = ObjectId.GenerateNewId().ToString(),
            SessionToken = "Session 1",
            UserId = "",
        };
        return Ok("Session added successfully");
    }
}