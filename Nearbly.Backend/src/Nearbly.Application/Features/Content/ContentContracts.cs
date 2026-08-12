using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Content;

public sealed record CreateProductRequest(string Name, string? Description, Guid MediaAssetId, decimal? Price, bool IsAvailable = true, int SortOrder = 0);
public sealed record UpdateProductRequest(string Name, string? Description, Guid MediaAssetId, decimal? Price, bool IsAvailable = true, int SortOrder = 0, bool? IsActive = null);
public sealed record ProductResponse(Guid Id, Guid StoreId, Guid StoreTabId, string Name, string? Description, Guid MediaAssetId, string ImageUrl, decimal? Price, bool IsAvailable, int SortOrder, bool IsActive, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);

public sealed record CreateMarkdownBlockRequest(string? Title, string Markdown, int SortOrder = 0);
public sealed record UpdateMarkdownBlockRequest(string? Title, string Markdown, int SortOrder = 0, bool? IsActive = null);
public sealed record MarkdownBlockResponse(Guid Id, Guid StoreId, Guid StoreTabId, string? Title, string Markdown, int SortOrder, bool IsActive, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);

public sealed record CreateGalleryItemRequest(Guid MediaAssetId, string AltText, string? Caption, int SortOrder = 0);
public sealed record UpdateGalleryItemRequest(Guid MediaAssetId, string AltText, string? Caption, int SortOrder = 0, bool? IsActive = null);
public sealed record GalleryItemResponse(Guid Id, Guid StoreId, Guid StoreTabId, Guid MediaAssetId, string ImageUrl, string AltText, string? Caption, int SortOrder, bool IsActive, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);

public interface IContentService
{
    Task<IReadOnlyList<ProductResponse>> ListProductsAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken);
    Task<ProductResponse> GetProductAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken);
    Task<ProductResponse> CreateProductAsync(Guid storeId, Guid tabId, CreateProductRequest request, CancellationToken cancellationToken);
    Task<ProductResponse> UpdateProductAsync(Guid storeId, Guid tabId, Guid id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task DeactivateProductAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<MarkdownBlockResponse>> ListMarkdownBlocksAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken);
    Task<MarkdownBlockResponse> GetMarkdownBlockAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken);
    Task<MarkdownBlockResponse> CreateMarkdownBlockAsync(Guid storeId, Guid tabId, CreateMarkdownBlockRequest request, CancellationToken cancellationToken);
    Task<MarkdownBlockResponse> UpdateMarkdownBlockAsync(Guid storeId, Guid tabId, Guid id, UpdateMarkdownBlockRequest request, CancellationToken cancellationToken);
    Task DeactivateMarkdownBlockAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<GalleryItemResponse>> ListGalleryItemsAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken);
    Task<GalleryItemResponse> GetGalleryItemAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken);
    Task<GalleryItemResponse> CreateGalleryItemAsync(Guid storeId, Guid tabId, CreateGalleryItemRequest request, CancellationToken cancellationToken);
    Task<GalleryItemResponse> UpdateGalleryItemAsync(Guid storeId, Guid tabId, Guid id, UpdateGalleryItemRequest request, CancellationToken cancellationToken);
    Task DeactivateGalleryItemAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken);
}
