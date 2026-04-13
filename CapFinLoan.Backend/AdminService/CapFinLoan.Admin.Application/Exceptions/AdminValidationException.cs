namespace CapFinLoan.Admin.Application.Exceptions;

public sealed class AdminValidationException : AdminServiceException
{
    public AdminValidationException(string message)
        : base(message, 400, "ADMIN_VALIDATION_ERROR")
    {
    }
}