namespace CapFinLoan.Wallet.Application.Exceptions;

public sealed class WalletForbiddenException : WalletServiceException
{
    public WalletForbiddenException(string message = "You are not allowed to access this application.")
        : base(message, 403, "APPLICATION_FORBIDDEN")
    {
    }
}
