namespace CapFinLoan.Wallet.Application.Exceptions;

public sealed class InsufficientWalletBalanceException : WalletServiceException
{
    public decimal Required { get; }
    public decimal Available { get; }

    public InsufficientWalletBalanceException(decimal required, decimal available)
        : base(
            $"Insufficient wallet balance. Required â‚¹{required:N2}, but only â‚¹{available:N2} is available. Please top up your wallet and try again.",
            402,
            "INSUFFICIENT_WALLET_BALANCE")
    {
        Required = required;
        Available = available;
    }
}
