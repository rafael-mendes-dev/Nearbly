using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Tabs;

public sealed record CreateTabRequest(string Key, string Name, int SortOrder = 0, string? ContentType = null);
public sealed record UpdateTabRequest(string Key, string Name, int SortOrder = 0, bool? IsActive = null, string? ContentType = null);
public sealed record TabResponse(Guid Id, Guid StoreId, string Key, string Name, string ContentType, int SortOrder, bool IsActive, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc)
{
    public static TabResponse From(StoreTab tab) => new(tab.Id, tab.StoreId, tab.Key, tab.Name, tab.ContentType.ToWireValue(), tab.SortOrder, tab.IsActive, tab.CreatedAtUtc, tab.UpdatedAtUtc);
}

public interface ITabService
{
    Task<IReadOnlyList<TabResponse>> ListAsync(Guid storeId, CancellationToken cancellationToken);
    Task<TabResponse> GetAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken);
    Task<TabResponse> CreateAsync(Guid storeId, CreateTabRequest request, CancellationToken cancellationToken);
    Task<TabResponse> UpdateAsync(Guid storeId, Guid tabId, UpdateTabRequest request, CancellationToken cancellationToken);
    Task DeactivateAsync(Guid storeId, Guid tabId, CancellationToken cancellationToken);
}
