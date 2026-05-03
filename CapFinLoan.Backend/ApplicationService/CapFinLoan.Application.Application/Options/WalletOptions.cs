namespace CapFinLoan.Application.Application.Options;

public class WalletOptions
{
    public const string SectionName = "Wallet";

    public decimal ApplicationFee { get; set; } = 500;
    public decimal MinTopUpAmount { get; set; } = 100;
    public decimal MaxTopUpAmount { get; set; } = 500000;
    public string Currency { get; set; } = "INR";
}
