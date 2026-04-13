using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Admin.Application.Exceptions;

public abstract class AdminServiceException : Exception
{
    protected AdminServiceException(string message, int statusCode, string errorCode, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public int StatusCode { get; }

    public string ErrorCode { get; }
}