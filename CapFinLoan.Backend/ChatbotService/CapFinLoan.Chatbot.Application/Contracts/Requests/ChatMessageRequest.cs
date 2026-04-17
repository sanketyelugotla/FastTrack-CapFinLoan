namespace CapFinLoan.Chatbot.Application.Contracts.Requests;

public class ChatMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public List<ConversationEntry> ConversationHistory { get; set; } = new();
}

public class ConversationEntry
{
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
}
