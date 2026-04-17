namespace CapFinLoan.Chatbot.Application.Contracts.Responses;

public class ChatMessageResponse
{
    public string Reply { get; set; } = string.Empty;
    public List<ActionResult> ActionsTaken { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}

public class ActionResult
{
    public string Action { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // "success" or "failed"
    public string Details { get; set; } = string.Empty;
}
