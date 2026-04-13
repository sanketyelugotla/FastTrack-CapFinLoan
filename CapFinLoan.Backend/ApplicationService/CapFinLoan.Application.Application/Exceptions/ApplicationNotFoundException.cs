using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Application.Application.Exceptions;

public sealed class ApplicationNotFoundException : ApplicationServiceException
{
    public ApplicationNotFoundException(string message = "Application not found.")
        : base(message, StatusCodes.Status404NotFound, "APPLICATION_NOT_FOUND")
    {
    }
}