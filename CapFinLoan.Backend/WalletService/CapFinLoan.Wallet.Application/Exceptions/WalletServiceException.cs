using System.Net;

namespace CapFinLoan.Wallet.Application.Exceptions;

public class WalletServiceException : Exception
{
    public int StatusCode { get; }
    public string ErrorCode { get; }

    public WalletServiceException(string message, int statusCode = 500, string errorCode = "INTERNAL_ERROR") 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
