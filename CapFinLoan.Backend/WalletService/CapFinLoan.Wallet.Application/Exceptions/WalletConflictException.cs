namespace CapFinLoan.Wallet.Application.Exceptions;

public sealed class WalletConflictException : WalletServiceException
{
    public WalletConflictException(string message)
        : base(message, 409, "APPLICATION_CONFLICT")
    {
    }
}
