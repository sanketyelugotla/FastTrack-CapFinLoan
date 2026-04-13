using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Admin.Application.Exceptions;

public sealed class AdminValidationException : AdminServiceException
{
    public AdminValidationException(string message)
        : base(message, StatusCodes.Status400BadRequest, "ADMIN_VALIDATION_ERROR")
    {
    }
}