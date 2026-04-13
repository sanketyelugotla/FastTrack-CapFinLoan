namespace CapFinLoan.Admin.Application.Exceptions;

public sealed class AdminConflictException : AdminServiceException
{
    public AdminConflictException(string message)
        : base(message, 409, "ADMIN_CONFLICT")
    {
    }
}