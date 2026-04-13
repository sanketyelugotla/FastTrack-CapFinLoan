using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Document.Application.Exceptions;

public sealed class DocumentConflictException : DocumentServiceException
{
    public DocumentConflictException(string message)
        : base(message, StatusCodes.Status409Conflict, "DOCUMENT_CONFLICT")
    {
    }
}