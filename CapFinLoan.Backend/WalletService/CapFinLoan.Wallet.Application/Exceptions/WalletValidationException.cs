namespace CapFinLoan.Wallet.Application.Exceptions;

public sealed class WalletValidationException : WalletServiceException
{
    public WalletValidationException(string message)
        : base(message, 400, "APPLICATION_VALIDATION_ERROR")
    {
    }
}
