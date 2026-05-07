namespace CapFinLoan.Wallet.Domain.Entities;

public class WalletAccount
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public string Currency { get; set; } = "INR";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<WalletLedgerEntry> LedgerEntries { get; set; } = new List<WalletLedgerEntry>();
}
