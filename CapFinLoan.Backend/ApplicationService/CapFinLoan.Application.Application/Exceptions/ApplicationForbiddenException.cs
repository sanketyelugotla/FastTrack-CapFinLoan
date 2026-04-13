using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Application.Application.Exceptions;

public sealed class ApplicationForbiddenException : ApplicationServiceException
{
    public ApplicationForbiddenException(string message = "You are not allowed to access this application.")
        : base(message, StatusCodes.Status403Forbidden, "APPLICATION_FORBIDDEN")
    {
    }
}