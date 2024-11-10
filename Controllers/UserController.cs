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
    public async Task<IActionResult> UpdateUser(string userid, [FromBody] User updatedFields)
    {
        // Check if the user exists
        var existingUser = await _mongoDBService.Users.Find(u => u.UserId == userid).FirstOrDefaultAsync();
        if (existingUser == null)
        {
            return NotFound("User not found");
        }

        // Create a list of update definitions to update only provided fields
        var updateDefinition = new List<UpdateDefinition<User>>();

        if (!string.IsNullOrEmpty(updatedFields.Email))
            updateDefinition.Add(Builders<User>.Update.Set(u => u.Email, updatedFields.Email));

        if (!string.IsNullOrEmpty(updatedFields.Password))
            updateDefinition.Add(Builders<User>.Update.Set(u => u.Password, updatedFields.Password));

        if (!string.IsNullOrEmpty(updatedFields.FirstName))
            updateDefinition.Add(Builders<User>.Update.Set(u => u.FirstName, updatedFields.FirstName));

        if (!string.IsNullOrEmpty(updatedFields.LastName))
            updateDefinition.Add(Builders<User>.Update.Set(u => u.LastName, updatedFields.LastName));

        if (updatedFields.JournalEntries != null && updatedFields.JournalEntries.Count > 0)
            updateDefinition.Add(Builders<User>.Update.Set(u => u.JournalEntries, updatedFields.JournalEntries));

        // Combine all update definitions
        var combinedUpdate = Builders<User>.Update.Combine(updateDefinition);

        // Apply the update
        var result = await _mongoDBService.Users.UpdateOneAsync(u => u.UserId == userid, combinedUpdate);

        return result.ModifiedCount > 0 ? Ok("User updated successfully") : NotFound("Failed to update user");
    }

    // Delete a user by userid
    [HttpDelete("deleteUser/{userid}")]
    public async Task<IActionResult> DeleteUser(string userid)
    {
        // Delete all journal entries associated with the user
        var deleteJournalEntriesResult = await _mongoDBService.JournalEntries
            .DeleteManyAsync(j => j.UserId == userid);

        // Delete the user document
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
