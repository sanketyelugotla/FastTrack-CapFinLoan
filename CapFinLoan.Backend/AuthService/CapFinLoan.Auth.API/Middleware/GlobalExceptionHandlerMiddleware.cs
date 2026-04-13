using CapFinLoan.Auth.Application.Exceptions;

namespace CapFinLoan.Auth.API.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AuthServiceException ex)
        {
            logger.LogWarning(ex, "Auth exception {ErrorCode}: {Message}", ex.ErrorCode, ex.Message);
            await WriteErrorAsync(context, ex.StatusCode, ex.ErrorCode, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, StatusCodes.Status500InternalServerError, "UNHANDLED_EXCEPTION", "An unexpected error occurred. Please try again.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string errorCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            message,
            errorCode,
            statusCode,
            traceId = context.TraceIdentifier,
            timestampUtc = DateTime.UtcNow
        });
    }
}
