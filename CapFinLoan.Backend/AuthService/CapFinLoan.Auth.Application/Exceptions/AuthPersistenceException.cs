using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Auth.Application.Exceptions;

public sealed class AuthPersistenceException : AuthServiceException
{
    public AuthPersistenceException(string message, Exception? innerException = null)
        : base(message, StatusCodes.Status500InternalServerError, "AUTH_PERSISTENCE_ERROR", innerException)
    {
    }
}