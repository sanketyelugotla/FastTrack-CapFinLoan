using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Auth.Application.Exceptions;

public sealed class InvalidOtpException : AuthServiceException
{
    public InvalidOtpException(string message = "Invalid or expired OTP.")
        : base(message, StatusCodes.Status400BadRequest, "INVALID_OTP")
    {
    }
}