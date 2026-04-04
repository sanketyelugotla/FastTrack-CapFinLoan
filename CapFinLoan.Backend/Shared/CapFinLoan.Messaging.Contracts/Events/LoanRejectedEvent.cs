namespace CapFinLoan.Messaging.Contracts.Events;

public record LoanRejectedEvent
{
    public Guid ApplicationId { get; init; }
    public Guid ApplicantUserId { get; init; }
    public string ApplicationNumber { get; init; } = string.Empty;
    public string ApplicantName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public string Remarks { get; init; } = string.Empty;
    public Guid RejectedByUserId { get; init; }
    public DateTime RejectedAtUtc { get; init; }
}
