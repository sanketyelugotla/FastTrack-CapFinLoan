namespace CapFinLoan.Application.Application.Contracts.Responses;

public class WalletSummaryResponse
{
    public Guid WalletAccountId { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerType { get; set; } = string.Empty;
    public string Currency { get; set; } = "INR";
    public decimal Balance { get; set; }
    public IReadOnlyCollection<WalletLedgerEntryResponse> RecentEntries { get; set; } = Array.Empty<WalletLedgerEntryResponse>();
}
