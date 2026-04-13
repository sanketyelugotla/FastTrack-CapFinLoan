using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Document.Application.Exceptions;

public sealed class DocumentValidationException : DocumentServiceException
{
    public DocumentValidationException(string message)
        : base(message, StatusCodes.Status400BadRequest, "DOCUMENT_VALIDATION_ERROR")
    {
    }
}