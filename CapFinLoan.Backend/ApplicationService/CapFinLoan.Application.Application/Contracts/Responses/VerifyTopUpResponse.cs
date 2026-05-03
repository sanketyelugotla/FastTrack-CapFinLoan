namespace CapFinLoan.Application.Application.Contracts.Responses;

public class VerifyTopUpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public WalletSummaryResponse Wallet { get; set; } = new();
}
