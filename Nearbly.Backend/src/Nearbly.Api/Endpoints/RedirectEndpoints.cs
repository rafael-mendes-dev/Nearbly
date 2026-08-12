using Nearbly.Application.Features.Public;

namespace Nearbly.Api.Endpoints;

public static class RedirectEndpoints
{
    public static IEndpointRouteBuilder MapRedirectEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/r/{linkId:guid}", async (Guid linkId, string? src, IPublicService service, CancellationToken ct) =>
        {
            var target = await service.RegisterClickAsync(linkId, TrafficSourceParser.Parse(src), ct);
            return Results.Redirect(target.ToString(), permanent: false, preserveMethod: false);
        }).AllowAnonymous().WithTags("Redirects").WithSummary("Redirect to a tracked link").Produces(StatusCodes.Status302Found).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);
        return endpoints;
    }
}
