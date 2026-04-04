namespace CapFinLoan.Messaging.Contracts.Events;

public record LoanApprovedEvent
{
    public Guid ApplicationId { get; init; }
    public Guid ApplicantUserId { get; init; }
    public string ApplicationNumber { get; init; } = string.Empty;
    public string ApplicantName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public decimal? SanctionAmount { get; init; }
    public int RequestedTenureMonths { get; init; }
    public string Remarks { get; init; } = string.Empty;
    public Guid ApprovedByUserId { get; init; }
    public DateTime ApprovedAtUtc { get; init; }
}
