using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Nearbly.Api.Infrastructure;

public sealed class StatusCodeProblemDetailsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.HasStarted || context.Response.ContentLength is > 0 || !string.IsNullOrWhiteSpace(context.Response.ContentType))
            return;
        if (context.Response.StatusCode is not (StatusCodes.Status400BadRequest or StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden or StatusCodes.Status404NotFound or StatusCodes.Status429TooManyRequests))
            return;

        var (title, detail) = context.Response.StatusCode switch
        {
            StatusCodes.Status400BadRequest => ("Invalid request", "The request could not be understood."),
            StatusCodes.Status401Unauthorized => ("Authentication required", "A valid bearer token is required."),
            StatusCodes.Status403Forbidden => ("Forbidden", "You are not allowed to perform this operation."),
            StatusCodes.Status404NotFound => ("Not found", "The requested resource was not found."),
            _ => ("Too many requests", "Please wait before trying again.")
        };

        context.Response.ContentLength = null;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem), context.RequestAborted);
    }
}
