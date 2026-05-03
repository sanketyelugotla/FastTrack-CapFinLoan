namespace CapFinLoan.Application.Application.Exceptions;

public sealed class InsufficientWalletBalanceException : ApplicationServiceException
{
    public decimal Required { get; }
    public decimal Available { get; }

    public InsufficientWalletBalanceException(decimal required, decimal available)
        : base(
            $"Insufficient wallet balance. Required ₹{required:N2}, but only ₹{available:N2} is available. Please top up your wallet and try again.",
            402,
            "INSUFFICIENT_WALLET_BALANCE")
    {
        Required = required;
        Available = available;
    }
}
