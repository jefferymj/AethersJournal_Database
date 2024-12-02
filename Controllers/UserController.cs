using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using BCrypt.Net;

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
    public async Task<IActionResult> AddUser([FromBody] UserRequest user)
    {
        if (user == null || string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName) || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest("All fields are required");
        }

        // Hash password
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);
        User userModel = new User
        {
            _id = ObjectId.GenerateNewId().ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Password = hashedPassword,
        };
        await _mongoDBService.Users.InsertOneAsync(userModel);
        return Ok("Password hashed & User added successfully");

    }

    // Login using email and password
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
        {
            return BadRequest("Email and password are required");
        }

        var existingUser = await _mongoDBService.Users.Find(u => u.Email == loginRequest.Email).FirstOrDefaultAsync();
        if (existingUser == null)
        {
            return NotFound("User not found");
        }

        // Compare hashed password
        if (BCrypt.Net.BCrypt.Verify(loginRequest.Password, existingUser.Password))
        {
            return Ok("Login successful");
        }
        else
        {
            return BadRequest("Invalid password");
        }
    }

    // Get a user by userid
    [HttpGet("getUserByID/{userid}")]
    public async Task<IActionResult> GetUser(string userid)
    {
        var user = await _mongoDBService.Users.Find(u => u._id == userid).FirstOrDefaultAsync();
        return user != null ? Ok(user) : NotFound("User not found");
    }

    // Get a user by email
    [HttpGet("getUserByEmail/{email}")]
    public async Task<IActionResult> GetUserByEmail(string email)
    {
        var user = await _mongoDBService.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
        return user != null ? Ok(user) : NotFound("User not found");
    }

    // Update a user by userid
    [HttpPut("updateUser/{userid}")]
    public async Task<IActionResult> UpdateUser(string userid, [FromBody] User updatedFields)
    {
        // Check if the user exists
        var existingUser = await _mongoDBService.Users.Find(u => u._id == userid).FirstOrDefaultAsync();
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
        var result = await _mongoDBService.Users.UpdateOneAsync(u => u._id == userid, combinedUpdate);

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
            .DeleteOneAsync(u => u._id == userid);

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
