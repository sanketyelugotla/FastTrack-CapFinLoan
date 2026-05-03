namespace CapFinLoan.Application.Domain.Entities;

public class WalletLedgerEntry
{
    public Guid Id { get; set; }
    public Guid WalletAccountId { get; set; }
    public WalletAccount? WalletAccount { get; set; }

    public string Direction { get; set; } = string.Empty;
    public string EntryType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string Status { get; set; } = "Posted";
    public string ReferenceId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
