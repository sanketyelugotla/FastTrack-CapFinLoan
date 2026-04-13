using Microsoft.AspNetCore.Http;

namespace CapFinLoan.Application.Application.Exceptions;

public sealed class ApplicationValidationException : ApplicationServiceException
{
    public ApplicationValidationException(string message)
        : base(message, StatusCodes.Status400BadRequest, "APPLICATION_VALIDATION_ERROR")
    {
    }
}