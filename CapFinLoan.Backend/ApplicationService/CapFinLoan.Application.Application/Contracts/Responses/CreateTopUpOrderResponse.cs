namespace CapFinLoan.Application.Application.Contracts.Responses;

public class CreateTopUpOrderResponse
{
    public string Provider { get; set; } = "Razorpay";
    public string KeyId { get; set; } = string.Empty;
    public string ProviderOrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string ReferenceId { get; set; } = string.Empty;
}
