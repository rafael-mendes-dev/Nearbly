using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Nearbly.Application.Common;
using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Stores;

public sealed class StoreService(INearblyDbContext db, IValidator<CreateStoreRequest> createValidator, IValidator<UpdateStoreRequest> updateValidator, TimeProvider timeProvider) : IStoreService
{
    public async Task<IReadOnlyList<StoreResponse>> ListAsync(CancellationToken cancellationToken) =>
        await db.Stores.AsNoTracking().OrderBy(x => x.Name).ThenBy(x => x.Id).Select(x => new StoreResponse(x.Id, x.Name, x.Slug, x.Description, x.LogoMediaId.HasValue ? "/media/" + x.LogoMediaId : x.LogoUrl, x.PrimaryColor, x.SecondaryColor, x.LogoMediaId, x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc)).ToListAsync(cancellationToken);

    public async Task<StoreResponse> GetAsync(Guid storeId, CancellationToken cancellationToken) =>
        StoreResponse.From(await GetEntityAsync(storeId, cancellationToken));

    public async Task<StoreResponse> CreateAsync(CreateStoreRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var store = new Store(request.Name, request.Slug, request.Description, request.LogoUrl, request.PrimaryColor, request.SecondaryColor, timeProvider.GetUtcNow());
        if (request.LogoMediaId.HasValue)
        {
            if (!await db.MediaAssets.AnyAsync(x => x.Id == request.LogoMediaId && x.StoreId == store.Id && x.IsActive, cancellationToken))
                throw new ConflictException("The logo media does not belong to this store.");
            store.SetLogoMedia(request.LogoMediaId, timeProvider.GetUtcNow());
        }
        db.Stores.Add(store);
        await SaveAsync("A store with this slug already exists.", cancellationToken);
        return StoreResponse.From(store);
    }

    public async Task<StoreResponse> UpdateAsync(Guid storeId, UpdateStoreRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        var store = await GetEntityAsync(storeId, cancellationToken);
        store.Update(request.Name, request.Slug, request.Description, request.LogoUrl, request.PrimaryColor, request.SecondaryColor, timeProvider.GetUtcNow());
        if (request.LogoMediaId.HasValue)
        {
            if (!await db.MediaAssets.AnyAsync(x => x.Id == request.LogoMediaId && x.StoreId == storeId && x.IsActive, cancellationToken))
                throw new ConflictException("The logo media does not belong to this store.");
            store.SetLogoMedia(request.LogoMediaId, timeProvider.GetUtcNow());
        }
        if (request.IsActive is true) store.Activate();
        if (request.IsActive is false) store.Deactivate();
        await SaveAsync("A store with this slug already exists.", cancellationToken);
        return StoreResponse.From(store);
    }

    public async Task DeactivateAsync(Guid storeId, CancellationToken cancellationToken)
    {
        var store = await GetEntityAsync(storeId, cancellationToken);
        store.Deactivate();
        await SaveAsync("The store could not be updated.", cancellationToken);
    }

    private async Task<Store> GetEntityAsync(Guid storeId, CancellationToken cancellationToken) =>
        await db.Stores.SingleOrDefaultAsync(x => x.Id == storeId, cancellationToken)
        ?? throw new NotFoundException("Store not found.");

    private async Task SaveAsync(string conflictMessage, CancellationToken cancellationToken)
    {
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException) { throw new ConflictException(conflictMessage); }
    }
}
