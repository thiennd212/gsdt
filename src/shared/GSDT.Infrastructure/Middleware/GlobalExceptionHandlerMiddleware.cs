using System.Net;
using System.Text.Json;

namespace GSDT.Infrastructure.Middleware;

/// <summary>
/// Global exception handler — catches unhandled exceptions, returns RFC 9457 ProblemDetails.
/// Never leaks stack traces in production. Logs with correlation ID.
/// </summary>
public sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger,
    IHostEnvironment env)
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var correlationId = context.Items["X-Correlation-Id"]?.ToString() ?? Guid.NewGuid().ToString("N");

        logger.LogError(ex, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

        var (statusCode, errorCode, message) = ex switch
        {
            DbUpdateConcurrencyException => (HttpStatusCode.Conflict, "GOV_SYS_003", "Data was modified by another user. Please refresh and try again."),
            ArgumentException => (HttpStatusCode.BadRequest, "GOV_SYS_002", "Invalid request parameter."),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "GOV_SEC_002", "Unauthorized."),
            _ => (HttpStatusCode.InternalServerError, "GOV_SYS_000", "An unexpected error occurred.")
        };

        var problem = new
        {
            type = $"https://gov.vn/errors/{errorCode.ToLowerInvariant()}",
            title = message,
            status = (int)statusCode,
            detail = env.IsDevelopment() ? ex.Message : "Contact support.",
            instance = context.Request.Path.Value,
            errorCode,
            correlationId,
            timestamp = DateTimeOffset.UtcNow
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOpts));
    }
}

public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
}
