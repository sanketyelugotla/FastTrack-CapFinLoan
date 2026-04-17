namespace CapFinLoan.Chatbot.API.Middleware;

public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized access: {Message}", ex.Message);
            await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "UNAUTHORIZED", ex.Message);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "External service error: {Message}", ex.Message);
            await WriteErrorAsync(context, StatusCodes.Status502BadGateway, "EXTERNAL_SERVICE_ERROR", "An external service is unavailable. Please try again.");
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
