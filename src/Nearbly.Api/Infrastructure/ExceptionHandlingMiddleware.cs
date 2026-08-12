using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Nearbly.Application.Common;
using System.Text.Json;

namespace Nearbly.Api.Infrastructure;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await WriteProblemAsync(context, exception);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (status, title, detail) = exception switch
        {
            ValidationException validation => (StatusCodes.Status400BadRequest, "Validation failed", string.Join(" ", validation.Errors.Select(error => error.ErrorMessage))),
            ArgumentException argument => (StatusCodes.Status400BadRequest, "Invalid request", argument.Message),
            BadHttpRequestException badRequest => (StatusCodes.Status400BadRequest, "Invalid request", badRequest.Message),
            JsonException => (StatusCodes.Status400BadRequest, "Invalid JSON", "The request body contains invalid JSON."),
            ConflictException conflict => (StatusCodes.Status409Conflict, "Conflict", conflict.Message),
            NotFoundException notFound => (StatusCodes.Status404NotFound, "Not found", notFound.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", "The server could not complete the request.")
        };
        if (status >= 500)
            logger.LogError(exception, "Unhandled request failure for {Method} {Path}.", context.Request.Method, context.Request.Path);
        else
            logger.LogWarning("Request failed with status {StatusCode} for {Method} {Path}: {Detail}", status, context.Request.Method, context.Request.Path, detail);

        context.Response.StatusCode = status;
        context.Response.ContentLength = null;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails { Status = status, Title = title, Detail = detail, Instance = context.Request.Path };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem), context.RequestAborted);
    }
}
