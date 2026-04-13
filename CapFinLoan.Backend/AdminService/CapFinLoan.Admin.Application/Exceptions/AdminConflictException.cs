using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Admin.Application.Exceptions;

public sealed class AdminConflictException : AdminServiceException
{
    public AdminConflictException(string message)
        : base(message, StatusCodes.Status409Conflict, "ADMIN_CONFLICT")
    {
    }
}