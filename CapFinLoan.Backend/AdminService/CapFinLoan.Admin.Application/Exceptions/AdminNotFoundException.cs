namespace CapFinLoan.Admin.Application.Exceptions;

public sealed class AdminNotFoundException : AdminServiceException
{
    public AdminNotFoundException(string message = "Application not found.")
        : base(message, 404, "ADMIN_NOT_FOUND")
    {
    }
}