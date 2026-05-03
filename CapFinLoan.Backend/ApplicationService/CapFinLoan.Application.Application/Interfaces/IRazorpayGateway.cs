namespace CapFinLoan.Application.Application.Interfaces;

public interface IRazorpayGateway
{
    Task<string> CreateOrderAsync(decimal amount, string currency, string receipt, CancellationToken cancellationToken = default);
    bool VerifySignature(string orderId, string paymentId, string signature);
    string GetPublicKey();
}
