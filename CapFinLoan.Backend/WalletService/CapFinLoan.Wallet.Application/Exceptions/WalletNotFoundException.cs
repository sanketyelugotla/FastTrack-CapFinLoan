namespace CapFinLoan.Wallet.Application.Exceptions;

public sealed class WalletNotFoundException : WalletServiceException
{
    public WalletNotFoundException(string message = "Application not found.")
        : base(message, 404, "APPLICATION_NOT_FOUND")
    {
    }
}
