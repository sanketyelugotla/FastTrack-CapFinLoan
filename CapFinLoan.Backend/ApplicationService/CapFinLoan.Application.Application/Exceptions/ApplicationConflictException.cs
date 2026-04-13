using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Application.Application.Exceptions;

public sealed class ApplicationConflictException : ApplicationServiceException
{
    public ApplicationConflictException(string message)
        : base(message, StatusCodes.Status409Conflict, "APPLICATION_CONFLICT")
    {
    }
}