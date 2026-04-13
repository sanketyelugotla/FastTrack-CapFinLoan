namespace CapFinLoan.Application.Application.Exceptions;

public sealed class ApplicationNotFoundException : ApplicationServiceException
{
    public ApplicationNotFoundException(string message = "Application not found.")
        : base(message, 404, "APPLICATION_NOT_FOUND")
    {
    }
}