using Nearbly.Application.Features.Public;

namespace Nearbly.Api.Endpoints;

public static class PublicEndpoints
{
    public static IEndpointRouteBuilder MapPublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/public").WithTags("Public");
        group.MapGet("/stores/{slug}", async (string slug, IPublicService service, CancellationToken ct) =>
        {
            var response = await service.GetStoreAsync(slug, ct);
            return response is null ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found", detail: "Store not found.") : Results.Ok(response);
        }).AllowAnonymous().WithSummary("Get a public store page").Produces<PublicStoreResponse>().ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/stores/{slug}/views", async (string slug, RegisterPageViewRequest? request, IPublicService service, CancellationToken ct) =>
        {
            await service.RegisterViewAsync(slug, request ?? new RegisterPageViewRequest(), ct);
            return Results.NoContent();
        }).AllowAnonymous().WithSummary("Register a public page view").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status404NotFound);
        return endpoints;
    }
}
