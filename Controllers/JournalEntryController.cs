using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class JournalEntryController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;

    public JournalEntryController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
    }

    // Create a new journal entry
    [HttpPost("addJournalEntry/{userid}")]
    public async Task<IActionResult> AddOrUpdateJournalEntry(string userid, [FromBody] JournalEntryRequest journalEntryRequest)
    {
        // check if journal entry request fields are null
        if (string.IsNullOrEmpty(journalEntryRequest.Title) || string.IsNullOrEmpty(journalEntryRequest.Content) || string.IsNullOrEmpty(journalEntryRequest.Date))
        {
            return BadRequest("Title, Content, and Date are required fields.");
        }

        // Check if the associated user exists
        var existingUser = await _mongoDBService.Users.Find(u => u._id == userid).FirstOrDefaultAsync();


        if (existingUser == null)
        {
            // Return an error response if the associated user does not exist
            return NotFound("User does not exist. Cannot add or update journal entry.");
        }

        var parsedDate = DateTime.ParseExact(journalEntryRequest.Date, "yyyy-MM-dd", null);
        var nextDay = parsedDate.AddDays(1);

        // Check if a journal entry with the same ID already exists for this user
        var existingJournalEntry = await _mongoDBService.JournalEntries
            .Find(j => j.UserId == userid && j.Date >= parsedDate && j.Date < nextDay)
            .FirstOrDefaultAsync();

        if (existingJournalEntry != null)
        {
            // Partial update for an existing journal entry
            var updateDefinition = new List<UpdateDefinition<JournalEntry>>();

            if (!string.IsNullOrEmpty(journalEntryRequest.Title))
                updateDefinition.Add(Builders<JournalEntry>.Update.Set(j => j.Title, journalEntryRequest.Title));

            if (!string.IsNullOrEmpty(journalEntryRequest.Content))
                updateDefinition.Add(Builders<JournalEntry>.Update.Set(j => j.Content, journalEntryRequest.Content));

            // update summary now aaaaa
            var combinedUpdate = Builders<JournalEntry>.Update.Combine(updateDefinition);

            var updateResult = await _mongoDBService.JournalEntries.UpdateOneAsync(
                j => j.UserId == userid && j.Date == DateTime.Parse(journalEntryRequest.Date),
                combinedUpdate
            );

            return updateResult.ModifiedCount > 0 ? Ok("Journal entry updated successfully") : NotFound("Failed to update journal entry");
        }
        else
        {
            JournalEntry journalEntry = new JournalEntry
            {
                UserId = userid,
                Title = journalEntryRequest.Title,
                Content = journalEntryRequest.Content,
                Date = DateTime.Parse(journalEntryRequest.Date),
                Summary = "hi" // call summary function and pass in content
            };

            // Add a new journal entry if it doesn't already exist
            await _mongoDBService.JournalEntries.InsertOneAsync(journalEntry);

            // Update the user's document to add the new journalEntryId
            var addToUserJournalEntries = Builders<User>.Update.AddToSet(u => u.JournalEntries, journalEntry.Id);
            await _mongoDBService.Users.UpdateOneAsync(u => u._id == userid, addToUserJournalEntries);

            return Ok("Journal entry added successfully");
        }
    }

    // Get journal entry by journalentryid
    [HttpGet("getJournalEntry/{userid}/{date}")]
    public async Task<IActionResult> GetJournalEntry(string userid, string date)
    {   
        var user = _mongoDBService.Users.Find(u => u._id == userid).FirstOrDefault();
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Retrieve the journal entry that matches the specified userid and journalentryid
        var journalEntry = await _mongoDBService.JournalEntries
            .Find(j => j.UserId == userid && j.Date == DateTime.Parse(date))
            .FirstOrDefaultAsync();
        
        if (journalEntry == null)
        {
            // Return a 200 Ok response if no matching journal entry is found
            return Ok("There is no journal entry for that date.");
        }

        // Return the journal entry data as a JSON object
        return Ok(journalEntry);
    }

    // Update journal entry by journalentryid
    [HttpPut("updateJournalEntry/{userid}/{journalentryid}")]
    public async Task<IActionResult> UpdateJournalEntry(string userid, string journalentryid, [FromBody] JournalEntry updatedFields)
    {
        // Check if the journal entry exists and is associated with the specified user
        var existingJournalEntry = await _mongoDBService.JournalEntries
            .Find(j => j.UserId == userid)
            .FirstOrDefaultAsync();

        if (existingJournalEntry == null)
        {
            // Return an error response if the journal entry is not found or does not belong to the user
            return NotFound("Journal entry not found or does not belong to this user. Cannot update.");
        }

        // Define the update definition, only updating the fields that are provided
        var updateDefinition = new List<UpdateDefinition<JournalEntry>>();

        if (!string.IsNullOrEmpty(updatedFields.Title))
            updateDefinition.Add(Builders<JournalEntry>.Update.Set(j => j.Title, updatedFields.Title));

        if (updatedFields.Date != default(DateTime))
            updateDefinition.Add(Builders<JournalEntry>.Update.Set(j => j.Date, updatedFields.Date));

        if (!string.IsNullOrEmpty(updatedFields.Content))
            updateDefinition.Add(Builders<JournalEntry>.Update.Set(j => j.Content, updatedFields.Content));

        if (!string.IsNullOrEmpty(updatedFields.Summary))
            updateDefinition.Add(Builders<JournalEntry>.Update.Set(j => j.Summary, updatedFields.Summary));

        if (!string.IsNullOrEmpty(updatedFields.ChatId))
            updateDefinition.Add(Builders<JournalEntry>.Update.Set(j => j.ChatId, updatedFields.ChatId));

        // Combine the update definitions
        var combinedUpdate = Builders<JournalEntry>.Update.Combine(updateDefinition);

        // Apply the update
        var result = await _mongoDBService.JournalEntries.UpdateOneAsync(
            j => j.UserId == userid,
            combinedUpdate
        );

        return result.ModifiedCount > 0 ? Ok("Journal entry updated successfully") : NotFound("Failed to update journal entry");
    }

    // Delete journal entry by journalentryid
    [HttpDelete("deleteJournalEntry/{userid}/{journalentryid}")]
    public async Task<IActionResult> DeleteJournalEntry(string userid, string journalentryid)
    {
        // Check if the journal entry exists and belongs to the specified user
        var existingJournalEntry = await _mongoDBService.JournalEntries
            .Find(j => j.UserId == userid)
            .FirstOrDefaultAsync();

        if (existingJournalEntry == null)
        {
            // Return an error response if the journal entry is not found or does not belong to the user
            return NotFound("Journal entry not found or does not belong to this user. Cannot delete.");
        }

        // Delete the journal entry
        var deleteResult = await _mongoDBService.JournalEntries.DeleteOneAsync(
            j => j.UserId == userid
        );

        if (deleteResult.DeletedCount > 0)
        {
            // Update the user's document to remove the journalEntryId
            var pullFromUserJournalEntries = Builders<User>.Update.Pull(u => u.JournalEntries, journalentryid);
            await _mongoDBService.Users.UpdateOneAsync(u => u._id == userid, pullFromUserJournalEntries);

            return Ok("Journal entry deleted successfully");
        }

        return NotFound("Failed to delete journal entry");
    }
}
