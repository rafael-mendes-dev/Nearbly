using Nearbly.Application.Features.Auth;
using FluentValidation;

namespace Nearbly.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/admin/auth/login", async (LoginRequest request, IAuthService auth, IValidator<LoginRequest> validator, CancellationToken cancellationToken) =>
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);
            var response = await auth.LoginAsync(request, cancellationToken);
            return response is null ? Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Authentication failed", detail: "Invalid credentials.") : Results.Ok(response);
        }).AllowAnonymous().RequireRateLimiting("login").WithTags("Auth").WithSummary("Authenticate an administrator").WithDescription("Returns a short-lived JWT bearer token for the configured administrator account.").Produces<LoginResponse>().ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status429TooManyRequests);
        return endpoints;
    }
}
