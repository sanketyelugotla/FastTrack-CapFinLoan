namespace CapFinLoan.Application.Application.Contracts.Responses;

public class WalletLedgerEntryResponse
{
    public Guid Id { get; set; }
    public string Direction { get; set; } = string.Empty;
    public string EntryType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string Status { get; set; } = string.Empty;
    public string ReferenceId { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
