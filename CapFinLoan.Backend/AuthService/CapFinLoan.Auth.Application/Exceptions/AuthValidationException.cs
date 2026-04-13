using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Auth.Application.Exceptions;

public sealed class AuthValidationException : AuthServiceException
{
    public AuthValidationException(string message)
        : base(message, StatusCodes.Status400BadRequest, "AUTH_VALIDATION_ERROR")
    {
    }
}