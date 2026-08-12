using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Stores;

public sealed record CreateStoreRequest(
    string Name,
    string Slug,
    string? Description = null,
    string? LogoUrl = null,
    string? PrimaryColor = null,
    string? SecondaryColor = null);

public sealed record UpdateStoreRequest(
    string Name,
    string Slug,
    string? Description = null,
    string? LogoUrl = null,
    string? PrimaryColor = null,
    string? SecondaryColor = null,
    bool? IsActive = null);

public sealed record StoreResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? LogoUrl,
    string? PrimaryColor,
    string? SecondaryColor,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc)
{
    public static StoreResponse From(Store store) => new(store.Id, store.Name, store.Slug, store.Description, store.LogoUrl, store.PrimaryColor, store.SecondaryColor, store.IsActive, store.CreatedAtUtc, store.UpdatedAtUtc);
}

public interface IStoreService
{
    Task<IReadOnlyList<StoreResponse>> ListAsync(CancellationToken cancellationToken);
    Task<StoreResponse> GetAsync(Guid storeId, CancellationToken cancellationToken);
    Task<StoreResponse> CreateAsync(CreateStoreRequest request, CancellationToken cancellationToken);
    Task<StoreResponse> UpdateAsync(Guid storeId, UpdateStoreRequest request, CancellationToken cancellationToken);
    Task DeactivateAsync(Guid storeId, CancellationToken cancellationToken);
}
