using Nearbly.Application.Features.Analytics;
using Nearbly.Application.Features.Links;
using Nearbly.Application.Features.Stores;
using Nearbly.Application.Features.Tabs;

namespace Nearbly.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var stores = endpoints.MapGroup("/api/admin/stores").RequireAuthorization();
        stores.WithTags("Admin - Stores");
        stores.MapGet("", (IStoreService service, CancellationToken ct) => service.ListAsync(ct)).WithSummary("List stores").Produces<IReadOnlyList<StoreResponse>>().ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status403Forbidden);
        stores.MapGet("/{storeId:guid}", (Guid storeId, IStoreService service, CancellationToken ct) => service.GetAsync(storeId, ct)).WithSummary("Get a store").Produces<StoreResponse>().ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);
        stores.MapPost("", async (CreateStoreRequest request, IStoreService service, CancellationToken ct) =>
        {
            var response = await service.CreateAsync(request, ct);
            return Results.Created($"/api/admin/stores/{response.Id}", response);
        }).WithSummary("Create a store").Produces<StoreResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status409Conflict);
        stores.MapPut("/{storeId:guid}", async (Guid storeId, UpdateStoreRequest request, IStoreService service, CancellationToken ct) => Results.Ok(await service.UpdateAsync(storeId, request, ct))).WithSummary("Update a store").Produces<StoreResponse>().ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);
        stores.MapDelete("/{storeId:guid}", async (Guid storeId, IStoreService service, CancellationToken ct) => { await service.DeactivateAsync(storeId, ct); return Results.NoContent(); }).WithSummary("Deactivate a store").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);

        var tabs = endpoints.MapGroup("/api/admin/stores/{storeId:guid}/tabs").RequireAuthorization().WithTags("Admin - Tabs");
        tabs.MapGet("", (Guid storeId, ITabService service, CancellationToken ct) => service.ListAsync(storeId, ct)).WithSummary("List store tabs").Produces<IReadOnlyList<TabResponse>>().ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);
        tabs.MapGet("/{tabId:guid}", (Guid storeId, Guid tabId, ITabService service, CancellationToken ct) => service.GetAsync(storeId, tabId, ct)).WithSummary("Get a store tab").Produces<TabResponse>().ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);
        tabs.MapPost("", async (Guid storeId, CreateTabRequest request, ITabService service, CancellationToken ct) =>
        {
            var response = await service.CreateAsync(storeId, request, ct);
            return Results.Created($"/api/admin/stores/{storeId}/tabs/{response.Id}", response);
        }).WithSummary("Create a store tab").Produces<TabResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);
        tabs.MapPut("/{tabId:guid}", async (Guid storeId, Guid tabId, UpdateTabRequest request, ITabService service, CancellationToken ct) => Results.Ok(await service.UpdateAsync(storeId, tabId, request, ct))).WithSummary("Update a store tab").Produces<TabResponse>().ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);
        tabs.MapDelete("/{tabId:guid}", async (Guid storeId, Guid tabId, ITabService service, CancellationToken ct) => { await service.DeactivateAsync(storeId, tabId, ct); return Results.NoContent(); }).WithSummary("Deactivate a store tab").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);

        var links = endpoints.MapGroup("/api/admin/stores/{storeId:guid}/links").RequireAuthorization().WithTags("Admin - Links");
        links.MapGet("", (Guid storeId, ILinkService service, CancellationToken ct) => service.ListAsync(storeId, ct)).WithSummary("List store links").Produces<IReadOnlyList<LinkResponse>>().ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);
        links.MapGet("/{linkId:guid}", (Guid storeId, Guid linkId, ILinkService service, CancellationToken ct) => service.GetAsync(storeId, linkId, ct)).WithSummary("Get a store link").Produces<LinkResponse>().ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);
        links.MapPost("", async (Guid storeId, CreateLinkRequest request, ILinkService service, CancellationToken ct) =>
        {
            var response = await service.CreateAsync(storeId, request, ct);
            return Results.Created($"/api/admin/stores/{storeId}/links/{response.Id}", response);
        }).WithSummary("Create a store link").Produces<LinkResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);
        links.MapPut("/{linkId:guid}", async (Guid storeId, Guid linkId, UpdateLinkRequest request, ILinkService service, CancellationToken ct) => Results.Ok(await service.UpdateAsync(storeId, linkId, request, ct))).WithSummary("Update a store link").Produces<LinkResponse>().ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);
        links.MapDelete("/{linkId:guid}", async (Guid storeId, Guid linkId, ILinkService service, CancellationToken ct) => { await service.DeactivateAsync(storeId, linkId, ct); return Results.NoContent(); }).WithSummary("Deactivate a store link").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);

        var analytics = endpoints.MapGet("/api/admin/stores/{storeId:guid}/analytics", async (Guid storeId, DateOnly? from, DateOnly? to, IAnalyticsService service, CancellationToken ct) => Results.Ok(await service.GetAsync(storeId, from, to, ct))).RequireAuthorization().WithTags("Admin - Analytics");
        analytics.WithSummary("Get store analytics").Produces<StoreAnalyticsResponse>().ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);
        return endpoints;
    }
}
