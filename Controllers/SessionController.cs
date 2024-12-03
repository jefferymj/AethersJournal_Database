using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

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
    [HttpPost("createSession/{userId}")]
    async public Task<IActionResult> CreateUserSession(string userId)
    {
        string sessionToken = GenerateSecureToken(32);

        var filter = Builders<Session>.Filter.Eq(s => s.UserId, userId);
        var update = Builders<Session>.Update.Set(s => s.SessionToken, sessionToken);

        var sessionExists = await _mongoDBService.Session.Find(filter).FirstOrDefaultAsync();

        if (sessionExists != null)
        {
            var updateResult = await _mongoDBService.Session.UpdateOneAsync(filter, update);

            if (updateResult.ModifiedCount > 0)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(1),
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None
                };

                Response.Cookies.Append("sid", sessionToken, cookieOptions);

                return Ok("Session token updated successfully");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update session token");
            }
        }
        else
        {
            Session session = new Session
            {
                SessionToken = sessionToken,
                UserId = userId,
            };

            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(1),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("sid", sessionToken, cookieOptions);

            await _mongoDBService.Session.InsertOneAsync(session);
            return Ok("Session added successfully");
        }

    }

    // Create a new session
    [HttpPost("validateSession")]
    async public Task<IActionResult> validateUserSession()
    {
        if (Request.Cookies.TryGetValue("sid", out string? sessionToken))
        {
            var session = await _mongoDBService.Session
                .Find(s =>s.SessionToken == sessionToken)
                .FirstOrDefaultAsync();

            if (session != null)
            {
                return Ok("Session is valid");
            }
            else
            {
                return Unauthorized(new {sid =  sessionToken});
            }
        }
        else
        {
            return Unauthorized("Session cookie not found");
        }
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

    public static string GenerateSecureToken(int length)
    {
        var byteArray = new byte[length];
        RandomNumberGenerator.Fill(byteArray);
        return Convert.ToBase64String(byteArray);
    }
}