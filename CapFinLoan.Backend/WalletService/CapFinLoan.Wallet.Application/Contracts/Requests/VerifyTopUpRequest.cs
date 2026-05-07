namespace CapFinLoan.Wallet.Application.Contracts.Requests;

public class VerifyTopUpRequest
{
    public string ProviderOrderId { get; set; } = string.Empty;
    public string ProviderPaymentId { get; set; } = string.Empty;
    public string ProviderSignature { get; set; } = string.Empty;
}
