using Microsoft.EntityFrameworkCore;
using Nearbly.Application.Common;
using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Public;

public sealed class PublicService(INearblyDbContext db, TimeProvider timeProvider) : IPublicService
{
    public async Task<PublicStoreResponse?> GetStoreAsync(string identifier, CancellationToken cancellationToken)
    {
        var stores = PublicStores(identifier);
        return await stores.AsNoTracking()
            .Select(store => new PublicStoreResponse(
                store.Id,
                store.Name,
                store.Slug,
                store.PublicCode,
                store.Description,
                store.LogoMediaId.HasValue ? "/media/" + store.LogoMediaId : store.LogoUrl,
                store.PrimaryColor,
                store.SecondaryColor,
                store.Links.Where(link => link.IsActive && link.StoreTabId == null).OrderBy(link => link.SortOrder).ThenBy(link => link.Id).Select(link => new PublicLinkResponse(link.Id, link.Type, link.Label, link.Icon, "/r/" + link.Id)).ToList(),
                store.Tabs.Where(tab => tab.IsActive).OrderBy(tab => tab.SortOrder).ThenBy(tab => tab.Id).Select(tab => new PublicTabResponse(
                    tab.Id,
                    tab.Key,
                    tab.Name,
                    tab.ContentType.ToString().ToLower(),
                    tab.SortOrder,
                    tab.Links.Where(link => link.IsActive).OrderBy(link => link.SortOrder).ThenBy(link => link.Id).Select(link => new PublicLinkResponse(link.Id, link.Type, link.Label, link.Icon, "/r/" + link.Id)).ToList(),
                    tab.Products.Where(product => product.IsActive).OrderBy(product => product.SortOrder).ThenBy(product => product.Id).Select(product => new PublicProductResponse(product.Id, product.Name, product.Description, "/media/" + product.MediaAssetId, product.Price, product.IsAvailable, product.SortOrder)).ToList(),
                    tab.MarkdownBlocks.Where(block => block.IsActive).OrderBy(block => block.SortOrder).ThenBy(block => block.Id).Select(block => new PublicMarkdownBlockResponse(block.Id, block.Title, block.Markdown, block.SortOrder)).ToList(),
                    tab.GalleryItems.Where(item => item.IsActive).OrderBy(item => item.SortOrder).ThenBy(item => item.Id).Select(item => new PublicGalleryItemResponse(item.Id, "/media/" + item.MediaAssetId, item.AltText, item.Caption, item.SortOrder)).ToList())).ToList()))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task RegisterViewAsync(string identifier, RegisterPageViewRequest request, CancellationToken cancellationToken)
    {
        var storeId = await PublicStores(identifier).Select(x => (Guid?)x.Id).SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Store not found.");
        db.PageViews.Add(new PageView(storeId, request.Source ?? TrafficSource.Direct, timeProvider.GetUtcNow()));
        await db.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<Store> PublicStores(string identifier)
    {
        if (Store.HasPublicCodeFormat(identifier))
        {
            var publicCode = identifier.Trim().ToLowerInvariant();
            return db.Stores.Where(store => store.IsActive && store.PublicCode == publicCode);
        }

        var normalizedSlug = SlugNormalizer.Normalize(identifier);
        return db.Stores.Where(store => store.IsActive && store.Slug == normalizedSlug);
    }

    public async Task<Uri> RegisterClickAsync(Guid linkId, TrafficSource source, CancellationToken cancellationToken)
    {
        var link = await db.Links.Include(x => x.Store).SingleOrDefaultAsync(x => x.Id == linkId && x.IsActive && x.Store.IsActive, cancellationToken)
            ?? throw new NotFoundException("Link not found.");
        if (!UrlValidator.IsValid(link.Url) || !Uri.TryCreate(link.Url, UriKind.Absolute, out var target))
            throw new ConflictException("The link URL is invalid.");
        db.LinkClicks.Add(new LinkClick(link.StoreId, link.Id, source, timeProvider.GetUtcNow()));
        await db.SaveChangesAsync(cancellationToken);
        return target;
    }
}
