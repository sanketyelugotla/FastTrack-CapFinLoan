using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Application.Application.Exceptions;

public abstract class ApplicationServiceException : Exception
{
    protected ApplicationServiceException(string message, int statusCode, string errorCode, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public int StatusCode { get; }

    public string ErrorCode { get; }
}