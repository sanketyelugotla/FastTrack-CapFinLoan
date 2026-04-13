using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Admin.Application.Exceptions;

public sealed class AdminNotFoundException : AdminServiceException
{
    public AdminNotFoundException(string message = "Application not found.")
        : base(message, StatusCodes.Status404NotFound, "ADMIN_NOT_FOUND")
    {
    }
}