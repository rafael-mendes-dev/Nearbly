using Nearbly.Application.Features.Public;
using Nearbly.Application.Features.Media;

namespace Nearbly.Api.Endpoints;

public static class PublicEndpoints
{
    public static IEndpointRouteBuilder MapPublicEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/public").WithTags("Public");
        group.MapGet("/stores/{identifier}", async (string identifier, IPublicService service, CancellationToken ct) =>
        {
            var response = await service.GetStoreAsync(identifier, ct);
            return response is null ? Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Not found", detail: "Store not found.") : Results.Ok(response);
        }).AllowAnonymous().WithSummary("Get a public store page").Produces<PublicStoreResponse>().ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/stores/{identifier}/views", async (string identifier, RegisterPageViewRequest? request, IPublicService service, CancellationToken ct) =>
        {
            await service.RegisterViewAsync(identifier, request ?? new RegisterPageViewRequest(), ct);
            return Results.NoContent();
        }).AllowAnonymous().WithSummary("Register a public page view").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status404NotFound);
        endpoints.MapGet("/media/{mediaId:guid}", async (Guid mediaId, HttpContext context, IMediaService service, CancellationToken ct) =>
        {
            var file = await service.OpenReadAsync(mediaId, ct);
            if (file is null) return Results.NotFound();
            context.Response.Headers.CacheControl = "public,max-age=2592000,immutable";
            return Results.File(file.Content, "image/webp", enableRangeProcessing: true, entityTag: new Microsoft.Net.Http.Headers.EntityTagHeaderValue($"\"{mediaId:N}\""));
        }).AllowAnonymous().WithTags("Public").WithSummary("Serve optimized media").Produces(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound).CacheOutput("media");
        return endpoints;
    }
}
