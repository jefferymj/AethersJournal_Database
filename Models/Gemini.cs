public class SummaryResponse
{
    public required string response { get; set; }
}

public class ChatResponse
{
    public required string response { get; set; }
}

public class UserMessage
    {
        public required string Context { get; set; }
        public required string Message { get; set; }
    }
