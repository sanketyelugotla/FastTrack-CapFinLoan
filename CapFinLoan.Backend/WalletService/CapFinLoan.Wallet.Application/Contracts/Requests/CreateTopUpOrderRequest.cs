namespace CapFinLoan.Wallet.Application.Contracts.Requests;

public class CreateTopUpOrderRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
}
