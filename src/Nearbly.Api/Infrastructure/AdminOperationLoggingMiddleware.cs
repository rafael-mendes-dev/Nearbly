using System.Diagnostics;
using System.Security.Claims;

namespace Nearbly.Api.Infrastructure;

public sealed class AdminOperationLoggingMiddleware(RequestDelegate next, ILogger<AdminOperationLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var isAdminOperation = context.Request.Path.StartsWithSegments("/api/admin") &&
            !context.Request.Path.StartsWithSegments("/api/admin/auth/login");
        if (!isAdminOperation)
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var actor = context.User.FindFirstValue(ClaimTypes.Email)
                ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? "anonymous";
            var logLevel = context.Response.StatusCode >= StatusCodes.Status400BadRequest ? LogLevel.Warning : LogLevel.Information;
            logger.Log(logLevel, "Admin operation {Method} {Path} returned {StatusCode} for {Actor} in {ElapsedMilliseconds} ms.", context.Request.Method, context.Request.Path, context.Response.StatusCode, actor, stopwatch.ElapsedMilliseconds);
        }
    }
}
