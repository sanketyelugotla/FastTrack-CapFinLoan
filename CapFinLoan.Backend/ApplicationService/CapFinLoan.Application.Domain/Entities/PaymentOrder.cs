namespace CapFinLoan.Application.Domain.Entities;

public class PaymentOrder
{
    public Guid Id { get; set; }
    public Guid ApplicantUserId { get; set; }
    public Guid WalletAccountId { get; set; }
    public string Provider { get; set; } = "Razorpay";
    public string ProviderOrderId { get; set; } = string.Empty;
    public string ProviderPaymentId { get; set; } = string.Empty;
    public string ProviderSignature { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string Status { get; set; } = "Created";
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CapturedAtUtc { get; set; }
}
