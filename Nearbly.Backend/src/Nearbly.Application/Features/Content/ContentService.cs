using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Nearbly.Application.Common;
using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Content;

public sealed class ContentService(INearblyDbContext db, TimeProvider timeProvider) : IContentService
{
    public async Task<IReadOnlyList<ProductResponse>> ListProductsAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Products, cancellationToken);
        return await db.Products.AsNoTracking().Where(x => x.StoreId == storeId && x.StoreTabId == tabId).OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(ToProductResponse()).ToListAsync(cancellationToken);
    }

    public async Task<ProductResponse> GetProductAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Products, cancellationToken);
        var product = await db.Products.AsNoTracking().SingleOrDefaultAsync(x => x.StoreId == storeId && x.StoreTabId == tabId && x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");
        return ToProductResponse(product);
    }

    public async Task<ProductResponse> CreateProductAsync(Guid storeId, Guid tabId, CreateProductRequest request, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Products, cancellationToken);
        await EnsureMediaAsync(storeId, request.MediaAssetId, cancellationToken);
        var product = new Product(storeId, tabId, request.Name, request.Description, request.MediaAssetId, request.Price, request.IsAvailable, request.SortOrder, timeProvider.GetUtcNow());
        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);
        return ToProductResponse(product);
    }

    public async Task<ProductResponse> UpdateProductAsync(Guid storeId, Guid tabId, Guid id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Products, cancellationToken);
        var product = await db.Products.SingleOrDefaultAsync(x => x.StoreId == storeId && x.StoreTabId == tabId && x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Product not found.");
        await EnsureMediaAsync(storeId, request.MediaAssetId, cancellationToken);
        product.Update(request.Name, request.Description, request.MediaAssetId, request.Price, request.IsAvailable, request.SortOrder, timeProvider.GetUtcNow());
        if (request.IsActive is true) product.Activate(timeProvider.GetUtcNow());
        if (request.IsActive is false) product.Deactivate(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return ToProductResponse(product);
    }

    public Task DeactivateProductAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken) =>
        DeactivateAsync(db.Products, storeId, tabId, id, "Product not found.", cancellationToken);

    public async Task<IReadOnlyList<MarkdownBlockResponse>> ListMarkdownBlocksAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Markdown, cancellationToken);
        return await db.MarkdownBlocks.AsNoTracking().Where(x => x.StoreId == storeId && x.StoreTabId == tabId).OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new MarkdownBlockResponse(x.Id, x.StoreId, x.StoreTabId, x.Title, x.Markdown, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc)).ToListAsync(cancellationToken);
    }

    public async Task<MarkdownBlockResponse> GetMarkdownBlockAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Markdown, cancellationToken);
        return await db.MarkdownBlocks.AsNoTracking().Where(x => x.StoreId == storeId && x.StoreTabId == tabId && x.Id == id)
            .Select(x => new MarkdownBlockResponse(x.Id, x.StoreId, x.StoreTabId, x.Title, x.Markdown, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc)).SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Markdown block not found.");
    }

    public async Task<MarkdownBlockResponse> CreateMarkdownBlockAsync(Guid storeId, Guid tabId, CreateMarkdownBlockRequest request, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Markdown, cancellationToken);
        var block = new MarkdownBlock(storeId, tabId, request.Title, request.Markdown, request.SortOrder, timeProvider.GetUtcNow());
        db.MarkdownBlocks.Add(block);
        await db.SaveChangesAsync(cancellationToken);
        return new MarkdownBlockResponse(block.Id, block.StoreId, block.StoreTabId, block.Title, block.Markdown, block.SortOrder, block.IsActive, block.CreatedAtUtc, block.UpdatedAtUtc);
    }

    public async Task<MarkdownBlockResponse> UpdateMarkdownBlockAsync(Guid storeId, Guid tabId, Guid id, UpdateMarkdownBlockRequest request, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Markdown, cancellationToken);
        var block = await db.MarkdownBlocks.SingleOrDefaultAsync(x => x.StoreId == storeId && x.StoreTabId == tabId && x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Markdown block not found.");
        block.Update(request.Title, request.Markdown, request.SortOrder, timeProvider.GetUtcNow());
        if (request.IsActive is true) block.Activate(timeProvider.GetUtcNow());
        if (request.IsActive is false) block.Deactivate(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return new MarkdownBlockResponse(block.Id, block.StoreId, block.StoreTabId, block.Title, block.Markdown, block.SortOrder, block.IsActive, block.CreatedAtUtc, block.UpdatedAtUtc);
    }

    public Task DeactivateMarkdownBlockAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken) =>
        DeactivateAsync(db.MarkdownBlocks, storeId, tabId, id, "Markdown block not found.", cancellationToken);

    public async Task<IReadOnlyList<GalleryItemResponse>> ListGalleryItemsAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Gallery, cancellationToken);
        return await db.GalleryItems.AsNoTracking().Where(x => x.StoreId == storeId && x.StoreTabId == tabId).OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(ToGalleryResponse()).ToListAsync(cancellationToken);
    }

    public async Task<GalleryItemResponse> GetGalleryItemAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Gallery, cancellationToken);
        var item = await db.GalleryItems.AsNoTracking().SingleOrDefaultAsync(x => x.StoreId == storeId && x.StoreTabId == tabId && x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Gallery item not found.");
        return ToGalleryResponse(item);
    }

    public async Task<GalleryItemResponse> CreateGalleryItemAsync(Guid storeId, Guid tabId, CreateGalleryItemRequest request, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Gallery, cancellationToken);
        await EnsureMediaAsync(storeId, request.MediaAssetId, cancellationToken);
        var item = new GalleryItem(storeId, tabId, request.MediaAssetId, request.AltText, request.Caption, request.SortOrder, timeProvider.GetUtcNow());
        db.GalleryItems.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return ToGalleryResponse(item);
    }

    public async Task<GalleryItemResponse> UpdateGalleryItemAsync(Guid storeId, Guid tabId, Guid id, UpdateGalleryItemRequest request, CancellationToken cancellationToken)
    {
        await EnsureTabAsync(storeId, tabId, ContentType.Gallery, cancellationToken);
        var item = await db.GalleryItems.SingleOrDefaultAsync(x => x.StoreId == storeId && x.StoreTabId == tabId && x.Id == id, cancellationToken)
            ?? throw new NotFoundException("Gallery item not found.");
        await EnsureMediaAsync(storeId, request.MediaAssetId, cancellationToken);
        item.Update(request.MediaAssetId, request.AltText, request.Caption, request.SortOrder, timeProvider.GetUtcNow());
        if (request.IsActive is true) item.Activate(timeProvider.GetUtcNow());
        if (request.IsActive is false) item.Deactivate(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return ToGalleryResponse(item);
    }

    public Task DeactivateGalleryItemAsync(Guid storeId, Guid tabId, Guid id, CancellationToken cancellationToken) =>
        DeactivateAsync(db.GalleryItems, storeId, tabId, id, "Gallery item not found.", cancellationToken);

    private async Task EnsureTabAsync(Guid storeId, Guid tabId, ContentType type, CancellationToken cancellationToken)
    {
        var tab = await db.StoreTabs.SingleOrDefaultAsync(x => x.StoreId == storeId && x.Id == tabId, cancellationToken)
            ?? throw new NotFoundException("Tab not found.");
        if (tab.ContentType != type) throw new ConflictException($"The tab must have contentType {type.ToWireValue()}.");
    }

    private async Task EnsureMediaAsync(Guid storeId, Guid mediaId, CancellationToken cancellationToken)
    {
        if (!await db.MediaAssets.AnyAsync(x => x.Id == mediaId && x.StoreId == storeId && x.IsActive, cancellationToken))
            throw new ConflictException("The selected media does not belong to this store.");
    }

    private async Task DeactivateAsync<TEntity>(DbSet<TEntity> set, Guid storeId, Guid tabId, Guid id, string message, CancellationToken cancellationToken) where TEntity : class
    {
        var entity = await set.FindAsync([id], cancellationToken) ?? throw new NotFoundException(message);
        var storeProperty = entity switch
        {
            Product product => (product.StoreId, product.StoreTabId, (Action)(() => product.Deactivate(timeProvider.GetUtcNow()))),
            MarkdownBlock block => (block.StoreId, block.StoreTabId, (Action)(() => block.Deactivate(timeProvider.GetUtcNow()))),
            GalleryItem item => (item.StoreId, item.StoreTabId, (Action)(() => item.Deactivate(timeProvider.GetUtcNow()))),
            _ => throw new InvalidOperationException("Unsupported content entity.")
        };
        if (storeProperty.StoreId != storeId || storeProperty.StoreTabId != tabId) throw new NotFoundException(message);
        storeProperty.Item3();
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Expression<Func<Product, ProductResponse>> ToProductResponse() => x => new ProductResponse(x.Id, x.StoreId, x.StoreTabId, x.Name, x.Description, x.MediaAssetId, "/media/" + x.MediaAssetId, x.Price, x.IsAvailable, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc);
    private static Expression<Func<GalleryItem, GalleryItemResponse>> ToGalleryResponse() => x => new GalleryItemResponse(x.Id, x.StoreId, x.StoreTabId, x.MediaAssetId, "/media/" + x.MediaAssetId, x.AltText, x.Caption, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc);
    private static ProductResponse ToProductResponse(Product x) => new(x.Id, x.StoreId, x.StoreTabId, x.Name, x.Description, x.MediaAssetId, $"/media/{x.MediaAssetId}", x.Price, x.IsAvailable, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc);
    private static GalleryItemResponse ToGalleryResponse(GalleryItem x) => new(x.Id, x.StoreId, x.StoreTabId, x.MediaAssetId, $"/media/{x.MediaAssetId}", x.AltText, x.Caption, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc);
}
