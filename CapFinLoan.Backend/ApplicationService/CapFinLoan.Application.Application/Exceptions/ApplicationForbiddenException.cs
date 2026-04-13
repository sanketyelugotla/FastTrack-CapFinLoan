namespace CapFinLoan.Application.Application.Exceptions;

public sealed class ApplicationForbiddenException : ApplicationServiceException
{
    public ApplicationForbiddenException(string message = "You are not allowed to access this application.")
        : base(message, 403, "APPLICATION_FORBIDDEN")
    {
    }
}