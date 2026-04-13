using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Document.Application.Exceptions;

public abstract class DocumentServiceException : Exception
{
    protected DocumentServiceException(string message, int statusCode, string errorCode, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public int StatusCode { get; }

    public string ErrorCode { get; }
}