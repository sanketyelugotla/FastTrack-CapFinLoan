namespace CapFinLoan.Messaging.Contracts.Events;

/// <summary>
/// Published by NotificationService when email delivery fails.
/// Non-critical — does NOT trigger a rollback, only logged for alerting.
/// </summary>
public record NotificationFailedEvent
{
    public Guid ApplicationId { get; init; }
    public Guid ApplicantUserId { get; init; }
    public string ApplicationNumber { get; init; } = string.Empty;
    public string NotificationType { get; init; } = string.Empty;
    public string FailureReason { get; init; } = string.Empty;
    public DateTime FailedAtUtc { get; init; }
}
