namespace CapFinLoan.Application.Application.Exceptions;

public sealed class ApplicationConflictException : ApplicationServiceException
{
    public ApplicationConflictException(string message)
        : base(message, 409, "APPLICATION_CONFLICT")
    {
    }
}