using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Auth.Application.Exceptions;

public sealed class AuthNotFoundException : AuthServiceException
{
    public AuthNotFoundException(string message = "User not found.")
        : base(message, StatusCodes.Status404NotFound, "USER_NOT_FOUND")
    {
    }
}