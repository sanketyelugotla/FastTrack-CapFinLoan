using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Auth.Application.Exceptions;

public sealed class AccountDeactivatedException : AuthServiceException
{
    public AccountDeactivatedException(string message = "User is deactivated.")
        : base(message, StatusCodes.Status403Forbidden, "ACCOUNT_DEACTIVATED")
    {
    }
}