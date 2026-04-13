using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Document.Application.Exceptions;

public sealed class DocumentNotFoundException : DocumentServiceException
{
    public DocumentNotFoundException(string message = "Document not found.")
        : base(message, StatusCodes.Status404NotFound, "DOCUMENT_NOT_FOUND")
    {
    }
}