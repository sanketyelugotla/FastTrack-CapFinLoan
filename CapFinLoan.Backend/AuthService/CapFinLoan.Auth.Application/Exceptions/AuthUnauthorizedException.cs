using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Auth.Application.Exceptions;

public sealed class AuthUnauthorizedException : AuthServiceException
{
    public AuthUnauthorizedException(string message, Exception? innerException = null)
        : base(message, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", innerException)
    {
    }
}