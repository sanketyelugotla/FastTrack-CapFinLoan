namespace CapFinLoan.Application.Application.Exceptions;

public sealed class ApplicationValidationException : ApplicationServiceException
{
    public ApplicationValidationException(string message)
        : base(message, 400, "APPLICATION_VALIDATION_ERROR")
    {
    }
}