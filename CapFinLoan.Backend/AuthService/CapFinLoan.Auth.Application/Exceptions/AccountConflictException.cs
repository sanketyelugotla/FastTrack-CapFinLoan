using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Auth.Application.Exceptions;

public sealed class AccountConflictException : AuthServiceException
{
    public AccountConflictException(string message = "An account already exists with this email.")
        : base(message, StatusCodes.Status409Conflict, "ACCOUNT_CONFLICT")
    {
    }
}