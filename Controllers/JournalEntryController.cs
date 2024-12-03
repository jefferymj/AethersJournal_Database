using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

[ApiController]
[Route("api/[controller]")]
public class JournalEntryController : ControllerBase
{
    private readonly MongoDBService _mongoDBService;
    private readonly HttpClient _httpClient;

    public JournalEntryController(MongoDBService mongoDBService)
    {
        _mongoDBService = mongoDBService;
        _httpClient = new HttpClient();
    }

    // Create a new journal entry
    [HttpPost("addJournalEntry")]
    public async Task<IActionResult> AddOrUpdateJournalEntry([FromBody] JournalEntryRequest journalEntryRequest)
    {
        // check if journal entry request fields are null
        if (string.IsNullOrEmpty(journalEntryRequest.Title) || string.IsNullOrEmpty(journalEntryRequest.Content) || string.IsNullOrEmpty(journalEntryRequest.Date))
        {
            return BadRequest("Title, Content, and Date are required fields.");
        }

        if (Request.Cookies.TryGetValue("sid", out string? sessionToken))
        {
            Session session = await _mongoDBService.Session
                            .Find(s => s.SessionToken == sessionToken)
                            .FirstOrDefaultAsync();

            if (session == null) return Unauthorized();

            string userId = session.UserId;

            // Check if the associated user exists
            var existingUser = await _mongoDBService.Users.Find(u => u._id == userId).FirstOrDefaultAsync();

            if (existingUser == null)
            {
                // Return an error response if the associated user does not exist
                return NotFound("User does not exist. Cannot add or update journal entry.");
            }

            var parsedDate = DateTime.ParseExact(journalEntryRequest.Date, "yyyy-MM-dd", null);
            var nextDay = parsedDate.AddDays(1);

            // Check if a journal entry with the same ID already exists for this user
            JournalEntry existingJournalEntry = await _mongoDBService.JournalEntries
                .Find(j => j.UserId == userId && j.Date >= parsedDate && j.Date < nextDay)
                .FirstOrDefaultAsync();

            if (existingJournalEntry != null)
            {
                // Partial update for an existing journal entry
                var updateDefinition = new List<UpdateDefinition<JournalEntry>>();

                Chat chatExists = await _mongoDBService.Chat
                    .Find(c => c.JournalId == existingJournalEntry.Id)
                    .FirstOrDefaultAsync();

                if (chatExists == null)
                {
                    Chat chat = new()
                    {
                        JournalId = existingJournalEntry.Id,
                    };

                    Console.WriteLine(chat);

                    await _mongoDBService.Chat.InsertOneAsync(chat);
                    updateDefinition.Add(Builders<JournalEntry>.Update.Set(j => j.ChatId, chat.ChatId));
                }

                if (!string.IsNullOrEmpty(journalEntryRequest.Title))
                    updateDefinition.Add(Builders<JournalEntry>.Update.Set(j => j.Title, journalEntryRequest.Title));

                if (!string.IsNullOrEmpty(journalEntryRequest.Content))
                    updateDefinition.Add(Builders<JournalEntry>.Update.Set(j => j.Content, journalEntryRequest.Content));

                SummaryResponse journalSummary = await GetJournalSummary(journalEntryRequest.Content);

                var combinedUpdate = Builders<JournalEntry>.Update.Combine(updateDefinition);

                var updateResult = await _mongoDBService.JournalEntries.UpdateOneAsync(
                    j => j.UserId == userId && j.Date == DateTime.Parse(journalEntryRequest.Date),
                    combinedUpdate
                );

                return updateResult.ModifiedCount > 0 ? Ok(new {journalId = existingJournalEntry.Id}) : NotFound("Failed to update journal entry");
            }
            else
            {
                var journalSummary = await GetJournalSummary(journalEntryRequest.Content);

                JournalEntry journalEntry = new JournalEntry
                {
                    UserId = userId,
                    Title = journalEntryRequest.Title,
                    Content = journalEntryRequest.Content,
                    Date = DateTime.Parse(journalEntryRequest.Date),
                    Summary = journalSummary.response ?? ""
                };


                Chat chat = new()
                {
                    JournalId = journalEntry.Id,
                };

                await _mongoDBService.Chat.InsertOneAsync(chat);

                journalEntry.ChatId = chat._id;

                await _mongoDBService.JournalEntries.InsertOneAsync(journalEntry);

                var addJournalIdToChat = Builders<Chat>.Update.Set(c => c.JournalId, journalEntry.Id);
                await _mongoDBService.Chat.UpdateOneAsync(c => c._id == chat._id, addJournalIdToChat);

                var addToUserJournalEntries = Builders<User>.Update.AddToSet(u => u.JournalEntries, journalEntry.Id);
                await _mongoDBService.Users.UpdateOneAsync(u => u._id == userId, addToUserJournalEntries);

                return Ok(new {journalId = journalEntry.Id});
            }
        }
        return Unauthorized();
    }

    // Get journal entry by journalentryid
    [HttpGet("getJournalEntry/{date}")]
    public async Task<IActionResult> GetJournalEntry(string date)
    {
        if (Request.Cookies.TryGetValue("sid", out string? sessionToken))
        {
            Session session = await _mongoDBService.Session
                            .Find(s => s.SessionToken == sessionToken)
                            .FirstOrDefaultAsync();

            if (session == null) return Unauthorized();

            string userId = session.UserId;

            var user = _mongoDBService.Users.Find(u => u._id == userId).FirstOrDefault();
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Retrieve the journal entry that matches the specified userid and journalentryid
            var journalEntry = await _mongoDBService.JournalEntries
                .Find(j => j.UserId == userId && j.Date == DateTime.Parse(date))
                .FirstOrDefaultAsync();

            if (journalEntry == null)
            {
                // Return a 200 Ok response if no matching journal entry is found
                return Ok("There is no journal entry for that date.");
            }

            // Return the journal entry data as a JSON object
            return Ok(journalEntry);
        }
        return Unauthorized();
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

    public async Task<SummaryResponse> GetJournalSummary(string content)
    {
        var jsonObject = new { Message = content };
        var jsonContent = JsonSerializer.Serialize(jsonObject);
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://aether-czdxepa8htg7eec5.canadacentral-01.azurewebsites.net/api/summary/send-summary", httpContent);
        var responseContent = await response.Content.ReadAsStringAsync();

        var summaryResponse = JsonSerializer.Deserialize<SummaryResponse>(responseContent);
        return summaryResponse!;
    }
}
