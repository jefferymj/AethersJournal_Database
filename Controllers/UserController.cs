using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public UserController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    // Create a new user
    [HttpPost("addUser")]
    public async Task<IActionResult> AddUser([FromBody] User user)
    {
        if (ModelState.IsValid)
        {
            await _mongoDBService.Users.InsertOneAsync(user);
            return Ok("User added successfully");
        }
        return BadRequest(ModelState);
    }

    // Get a user by userid
    [HttpGet("getUser/{userid}")]
    public async Task<IActionResult> GetUser(string userid)
    {
        var user = await _mongoDBService.Users.Find(u => u.UserId == userid).FirstOrDefaultAsync();
        return user != null ? Ok(user) : NotFound("User not found");
    }

    // Update a user by userid
    [HttpPut("updateUser/{userid}")]
    public async Task<IActionResult> UpdateUser(string userid, [FromBody] User updatedUser)
    {
        var update = Builders<User>.Update
            .Set(u => u.Email, updatedUser.Email)
            .Set(u => u.Password, updatedUser.Password)
            .Set(u => u.JournalEntries, updatedUser.JournalEntries);

        var result = await _mongoDBService.Users.UpdateOneAsync(
            u => u.UserId == userid,
            update
        );

        return result.ModifiedCount > 0 ? Ok("User updated") : NotFound("User not found");
    }

    // Delete a user by userid
    [HttpDelete("deleteUser/{userid}")]
    public async Task<IActionResult> DeleteUser(string userid)
    {
        // Step 1: Delete all journal entries associated with the user
        var deleteJournalEntriesResult = await _mongoDBService.JournalEntries
            .DeleteManyAsync(j => j.UserId == userid);

        // Step 2: Delete the user document
        var deleteUserResult = await _mongoDBService.Users
            .DeleteOneAsync(u => u.UserId == userid);

        if (deleteUserResult.DeletedCount > 0)
        {
            return Ok($"User and {deleteJournalEntriesResult.DeletedCount} associated journal entries deleted successfully");
        }
        else
        {
            return NotFound("User not found");
        }
    }

}
