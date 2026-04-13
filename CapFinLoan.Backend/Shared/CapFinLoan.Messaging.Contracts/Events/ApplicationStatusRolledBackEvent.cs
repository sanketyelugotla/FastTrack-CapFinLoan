namespace CapFinLoan.Messaging.Contracts.Events;

public record ApplicationStatusRolledBackEvent
{
    public Guid ApplicationId { get; init; }
    public Guid ApplicantUserId { get; init; }
    public string ApplicationNumber { get; init; } = string.Empty;
    public string PreviousStatus { get; init; } = string.Empty; // The status before rollback
    public string RolledBackFromStatus { get; init; } = string.Empty; // The status that was rolled back
    public string Remarks { get; init; } = string.Empty;
    public Guid ChangedByUserId { get; init; }
    public DateTime ChangedAtUtc { get; init; }
}