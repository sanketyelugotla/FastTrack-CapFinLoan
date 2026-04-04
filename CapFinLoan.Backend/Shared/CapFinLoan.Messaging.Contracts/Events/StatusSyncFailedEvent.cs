namespace CapFinLoan.Messaging.Contracts.Events;

/// <summary>
/// Compensating event published by ApplicationService when it FAILS to sync
/// a status change from AdminService into its own database.
/// AdminService consumes this to roll back its decision.
/// </summary>
public record StatusSyncFailedEvent
{
    public Guid ApplicationId { get; init; }
    public Guid ApplicantUserId { get; init; }
    public string ApplicationNumber { get; init; } = string.Empty;
    public string AttemptedStatus { get; init; } = string.Empty;
    public string PreviousStatus { get; init; } = string.Empty;
    public string FailureReason { get; init; } = string.Empty;
    public DateTime FailedAtUtc { get; init; }
}
