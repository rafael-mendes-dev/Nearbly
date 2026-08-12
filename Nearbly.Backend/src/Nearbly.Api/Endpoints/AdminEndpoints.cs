using Nearbly.Application.Features.Analytics;
using Nearbly.Application.Common;
using Nearbly.Application.Features.Content;
using Nearbly.Application.Features.Links;
using Nearbly.Application.Features.Media;
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

        var media = endpoints.MapGroup("/api/admin/stores/{storeId:guid}/media").RequireAuthorization().WithTags("Admin - Media");
        media.MapPost("", async (Guid storeId, IFormFile file, IMediaService service, CancellationToken ct) =>
        {
            await using var stream = file.OpenReadStream();
            var response = await service.UploadAsync(storeId, new MediaUpload(stream, file.FileName, file.ContentType, file.Length), ct);
            return Results.Created($"/media/{response.Id}", response);
        }).DisableAntiforgery().WithSummary("Upload store media").Produces<MediaResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound);
        media.MapDelete("/{mediaId:guid}", async (Guid storeId, Guid mediaId, IMediaService service, CancellationToken ct) => { await service.DeactivateAsync(storeId, mediaId, ct); return Results.NoContent(); }).WithSummary("Deactivate unused media").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status401Unauthorized).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);

        var content = endpoints.MapGroup("/api/admin/stores/{storeId:guid}/tabs/{tabId:guid}").RequireAuthorization().WithTags("Admin - Content");
        content.MapGet("/products", (Guid storeId, Guid tabId, IContentService service, CancellationToken ct) => service.ListProductsAsync(storeId, tabId, ct)).Produces<IReadOnlyList<ProductResponse>>();
        content.MapGet("/products/{id:guid}", (Guid storeId, Guid tabId, Guid id, IContentService service, CancellationToken ct) => service.GetProductAsync(storeId, tabId, id, ct)).Produces<ProductResponse>();
        content.MapPost("/products", async (Guid storeId, Guid tabId, CreateProductRequest request, IContentService service, CancellationToken ct) => Results.Created($"/api/admin/stores/{storeId}/tabs/{tabId}/products", await service.CreateProductAsync(storeId, tabId, request, ct))).Produces<ProductResponse>(StatusCodes.Status201Created);
        content.MapPut("/products/{id:guid}", async (Guid storeId, Guid tabId, Guid id, UpdateProductRequest request, IContentService service, CancellationToken ct) => Results.Ok(await service.UpdateProductAsync(storeId, tabId, id, request, ct))).Produces<ProductResponse>();
        content.MapDelete("/products/{id:guid}", async (Guid storeId, Guid tabId, Guid id, IContentService service, CancellationToken ct) => { await service.DeactivateProductAsync(storeId, tabId, id, ct); return Results.NoContent(); });

        content.MapGet("/markdown-blocks", (Guid storeId, Guid tabId, IContentService service, CancellationToken ct) => service.ListMarkdownBlocksAsync(storeId, tabId, ct)).Produces<IReadOnlyList<MarkdownBlockResponse>>();
        content.MapGet("/markdown-blocks/{id:guid}", (Guid storeId, Guid tabId, Guid id, IContentService service, CancellationToken ct) => service.GetMarkdownBlockAsync(storeId, tabId, id, ct)).Produces<MarkdownBlockResponse>();
        content.MapPost("/markdown-blocks", async (Guid storeId, Guid tabId, CreateMarkdownBlockRequest request, IContentService service, CancellationToken ct) => Results.Created($"/api/admin/stores/{storeId}/tabs/{tabId}/markdown-blocks", await service.CreateMarkdownBlockAsync(storeId, tabId, request, ct))).Produces<MarkdownBlockResponse>(StatusCodes.Status201Created);
        content.MapPut("/markdown-blocks/{id:guid}", async (Guid storeId, Guid tabId, Guid id, UpdateMarkdownBlockRequest request, IContentService service, CancellationToken ct) => Results.Ok(await service.UpdateMarkdownBlockAsync(storeId, tabId, id, request, ct))).Produces<MarkdownBlockResponse>();
        content.MapDelete("/markdown-blocks/{id:guid}", async (Guid storeId, Guid tabId, Guid id, IContentService service, CancellationToken ct) => { await service.DeactivateMarkdownBlockAsync(storeId, tabId, id, ct); return Results.NoContent(); });

        content.MapGet("/gallery-items", (Guid storeId, Guid tabId, IContentService service, CancellationToken ct) => service.ListGalleryItemsAsync(storeId, tabId, ct)).Produces<IReadOnlyList<GalleryItemResponse>>();
        content.MapGet("/gallery-items/{id:guid}", (Guid storeId, Guid tabId, Guid id, IContentService service, CancellationToken ct) => service.GetGalleryItemAsync(storeId, tabId, id, ct)).Produces<GalleryItemResponse>();
        content.MapPost("/gallery-items", async (Guid storeId, Guid tabId, CreateGalleryItemRequest request, IContentService service, CancellationToken ct) => Results.Created($"/api/admin/stores/{storeId}/tabs/{tabId}/gallery-items", await service.CreateGalleryItemAsync(storeId, tabId, request, ct))).Produces<GalleryItemResponse>(StatusCodes.Status201Created);
        content.MapPut("/gallery-items/{id:guid}", async (Guid storeId, Guid tabId, Guid id, UpdateGalleryItemRequest request, IContentService service, CancellationToken ct) => Results.Ok(await service.UpdateGalleryItemAsync(storeId, tabId, id, request, ct))).Produces<GalleryItemResponse>();
        content.MapDelete("/gallery-items/{id:guid}", async (Guid storeId, Guid tabId, Guid id, IContentService service, CancellationToken ct) => { await service.DeactivateGalleryItemAsync(storeId, tabId, id, ct); return Results.NoContent(); });
        return endpoints;
    }
}
