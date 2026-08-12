using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nearbly.Application.Common;
using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Links;

public sealed class LinkService(INearblyDbContext db, IValidator<CreateLinkRequest> createValidator, IValidator<UpdateLinkRequest> updateValidator, TimeProvider timeProvider) : ILinkService
{
    public async Task<IReadOnlyList<LinkResponse>> ListAsync(Guid storeId, CancellationToken cancellationToken)
    {
        await EnsureStoreAsync(storeId, cancellationToken);
        return await db.Links.AsNoTracking().Where(x => x.StoreId == storeId).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).Select(x => new LinkResponse(x.Id, x.StoreId, x.StoreTabId, x.Type, x.Label, x.Icon, x.Url, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc)).ToListAsync(cancellationToken);
    }

    public async Task<LinkResponse> GetAsync(Guid storeId, Guid linkId, CancellationToken cancellationToken) =>
        LinkResponse.From(await GetEntityAsync(storeId, linkId, cancellationToken));

    public async Task<LinkResponse> CreateAsync(Guid storeId, CreateLinkRequest request, CancellationToken cancellationToken)
    {
        await EnsureStoreAsync(storeId, cancellationToken);
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await EnsureTabAsync(storeId, request.StoreTabId, cancellationToken);
        var link = new Link(storeId, request.Type, request.Label, request.Icon, request.Url, request.SortOrder, request.StoreTabId, timeProvider.GetUtcNow());
        db.Links.Add(link);
        await SaveAsync(cancellationToken);
        return LinkResponse.From(link);
    }

    public async Task<LinkResponse> UpdateAsync(Guid storeId, Guid linkId, UpdateLinkRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var link = await GetEntityAsync(storeId, linkId, cancellationToken);
        await EnsureTabAsync(storeId, request.StoreTabId, cancellationToken);
        link.Update(request.Type, request.Label, request.Icon, request.Url, request.SortOrder, request.StoreTabId, timeProvider.GetUtcNow());
        if (request.IsActive is true) link.Activate();
        if (request.IsActive is false) link.Deactivate();
        await SaveAsync(cancellationToken);
        return LinkResponse.From(link);
    }

    public async Task DeactivateAsync(Guid storeId, Guid linkId, CancellationToken cancellationToken)
    {
        var link = await GetEntityAsync(storeId, linkId, cancellationToken);
        link.Deactivate();
        await SaveAsync(cancellationToken);
    }

    private async Task<Link> GetEntityAsync(Guid storeId, Guid linkId, CancellationToken cancellationToken) =>
        await db.Links.SingleOrDefaultAsync(x => x.StoreId == storeId && x.Id == linkId, cancellationToken)
        ?? throw new NotFoundException("Link not found.");

    private async Task EnsureStoreAsync(Guid storeId, CancellationToken cancellationToken)
    {
        if (!await db.Stores.AnyAsync(x => x.Id == storeId, cancellationToken))
            throw new NotFoundException("Store not found.");
    }

    private async Task EnsureTabAsync(Guid storeId, Guid? tabId, CancellationToken cancellationToken)
    {
        if (tabId.HasValue)
        {
            var tab = await db.StoreTabs.SingleOrDefaultAsync(x => x.StoreId == storeId && x.Id == tabId.Value, cancellationToken)
                ?? throw new ConflictException("The selected tab does not belong to this store.");
            if (tab.ContentType != ContentType.Links) throw new ConflictException("Links can only be added to a links tab.");
        }
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { throw new ConflictException("The link could not be saved."); }
    }
}
