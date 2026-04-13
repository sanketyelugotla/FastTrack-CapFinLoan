using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Admin.Application.Exceptions;

public sealed class AdminForbiddenException : AdminServiceException
{
    public AdminForbiddenException(string message = "You are not allowed to access this application.")
        : base(message, StatusCodes.Status403Forbidden, "ADMIN_FORBIDDEN")
    {
    }
}