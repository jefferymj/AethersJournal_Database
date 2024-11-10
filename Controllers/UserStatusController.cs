using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class UserStatusController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public UserStatusController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    // Create a new user status
    [HttpPost("addUserStatus")]
    public async Task<IActionResult> AddUserStatus([FromBody] UserStatus userStatus)
    {
        // Check if the user exists in the users collection
        var existingUser = await _mongoDBService.Users.Find(u => u.UserId == userStatus.UserId).FirstOrDefaultAsync();
        if (existingUser == null)
        {
            // Return an error response if the user does not exist
            return NotFound("User does not exist. Cannot add user summary.");
        }

        // Check if a user status already exists for this user
        var existingUserStatus = await _mongoDBService.UserStatus.Find(us => us.UserId == userStatus.UserId).FirstOrDefaultAsync();
        if (existingUserStatus != null)
        {
            // Return an error response if a user status already exists for this user
            return Conflict("User status already exists for this user. Cannot add another.");
        }

        // If the user exists and no user status exists for this user, proceed to add the user status
        await _mongoDBService.UserStatus.InsertOneAsync(userStatus);
        return Ok("User status added successfully");
    }

    // Get user status by userid
    [HttpGet("getUserStatus/{userid}")]
    public async Task<IActionResult> GetUserStatus(string userid)
    {
        var status = await _mongoDBService.UserStatus.Find(us => us.UserId == userid).FirstOrDefaultAsync();
        return status != null ? Ok(status) : NotFound("User status not found");
    }

    // Update user status by userid
    [HttpPut("updateUserStatus/{userid}")]
    public async Task<IActionResult> UpdateUserStatus(string userid, [FromBody] UserStatus updatedStatus)
    {
        // Check if the user exists in the users collection
        var existingUser = await _mongoDBService.Users.Find(u => u.UserId == userid).FirstOrDefaultAsync();

        if (existingUser == null)
        {
            // Return an error response if the user does not exist
            return NotFound("User does not exist. Cannot update user summary.");
        }

        // Define the update definition for the user summary field
        var update = Builders<UserStatus>.Update.Set(us => us.UserSummary, updatedStatus.UserSummary);

        // Perform the partial update on the user status collection
        var result = await _mongoDBService.UserStatus.UpdateOneAsync(
            us => us.UserId == userid,
            update
        );

        return result.ModifiedCount > 0 ? Ok("User status updated") : NotFound("User status not found");
    }

    // Delete user status by userid
    [HttpDelete("deleteUserStatus/{userid}")]
    public async Task<IActionResult> DeleteUserStatus(string userid)
    {
        var result = await _mongoDBService.UserStatus.DeleteOneAsync(us => us.UserId == userid);
        return result.DeletedCount > 0 ? Ok("User status deleted") : NotFound("User status not found");
    }
}
