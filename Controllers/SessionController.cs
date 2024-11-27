using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public SessionController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    // Create a new session
    [HttpPost("getUser/useSession")]
    async public Task<IActionResult> GetUserUseSession()
    {
        var session = new Session
        {
            Id = ObjectId.GenerateNewId().ToString(),
            SessionToken = "Session 1",
            UserId = "",
        };
        return Ok("Session added successfully");
    }

    [HttpGet("test-cookie")]
    async public Task<IActionResult> TestCookie()
    {
        var cookieOptions = new CookieOptions
        {
            Expires = DateTime.UtcNow.AddDays(7),
            HttpOnly = true, 
            Secure = false,   
            SameSite = SameSiteMode.Lax 
        };

        Response.Cookies.Append("test-cookie", "COOOOKIE", cookieOptions);
        return Ok("Secure cookie has been set.");
    }
}