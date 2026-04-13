using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Document.Application.Exceptions;

public sealed class DocumentForbiddenException : DocumentServiceException
{
    public DocumentForbiddenException(string message = "You are not allowed to access this document.")
        : base(message, StatusCodes.Status403Forbidden, "DOCUMENT_FORBIDDEN")
    {
    }
}