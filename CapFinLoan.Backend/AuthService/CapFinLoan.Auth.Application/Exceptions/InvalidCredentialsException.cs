using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Auth.Application.Exceptions;

public sealed class InvalidCredentialsException : AuthServiceException
{
    public InvalidCredentialsException(string message = "Invalid email or password.", Exception? innerException = null)
        : base(message, StatusCodes.Status401Unauthorized, "INVALID_CREDENTIALS", innerException)
    {
    }
}