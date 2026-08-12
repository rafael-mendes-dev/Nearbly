using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nearbly.Application.Common;
using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Tabs;

public sealed class TabService(INearblyDbContext db, IValidator<CreateTabRequest> createValidator, IValidator<UpdateTabRequest> updateValidator, TimeProvider timeProvider) : ITabService
{
    public async Task<IReadOnlyList<TabResponse>> ListAsync(Guid storeId, CancellationToken cancellationToken)
    {
        await EnsureStoreAsync(storeId, cancellationToken);
        return await db.StoreTabs.AsNoTracking().Where(x => x.StoreId == storeId).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).Select(x => new TabResponse(x.Id, x.StoreId, x.Key, x.Name, x.SortOrder, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc)).ToListAsync(cancellationToken);
    }

    public async Task<TabResponse> GetAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken) =>
        TabResponse.From(await GetEntityAsync(storeId, tabId, cancellationToken));

    public async Task<TabResponse> CreateAsync(Guid storeId, CreateTabRequest request, CancellationToken cancellationToken)
    {
        await EnsureStoreAsync(storeId, cancellationToken);
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var tab = new StoreTab(storeId, request.Key, request.Name, request.SortOrder, timeProvider.GetUtcNow());
        db.StoreTabs.Add(tab);
        await SaveAsync(cancellationToken);
        return TabResponse.From(tab);
    }

    public async Task<TabResponse> UpdateAsync(Guid storeId, Guid tabId, UpdateTabRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var tab = await GetEntityAsync(storeId, tabId, cancellationToken);
        tab.Update(request.Key, request.Name, request.SortOrder, timeProvider.GetUtcNow());
        if (request.IsActive is true) tab.Activate();
        if (request.IsActive is false) tab.Deactivate();
        await SaveAsync(cancellationToken);
        return TabResponse.From(tab);
    }

    public async Task DeactivateAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken)
    {
        var tab = await GetEntityAsync(storeId, tabId, cancellationToken);
        tab.Deactivate();
        await SaveAsync(cancellationToken);
    }

    private async Task<StoreTab> GetEntityAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken) =>
        await db.StoreTabs.SingleOrDefaultAsync(x => x.StoreId == storeId && x.Id == tabId, cancellationToken)
        ?? throw new NotFoundException("Tab not found.");

    private async Task EnsureStoreAsync(Guid storeId, CancellationToken cancellationToken)
    {
        if (!await db.Stores.AnyAsync(x => x.Id == storeId, cancellationToken))
            throw new NotFoundException("Store not found.");
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { throw new ConflictException("A tab with this key already exists in the store."); }
    }
}
