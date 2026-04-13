using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Auth.Application.Exceptions;

public abstract class AuthServiceException : Exception
{
    protected AuthServiceException(string message, int statusCode, string errorCode, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public int StatusCode { get; }

    public string ErrorCode { get; }
}