namespace CapFinLoan.Messaging.Contracts.Events;

public class OtpSendEvent
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime SentAtUtc { get; set; }
}
