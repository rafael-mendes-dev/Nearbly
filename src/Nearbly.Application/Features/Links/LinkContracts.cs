using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Links;

public sealed record CreateLinkRequest(string Type, string Label, string? Icon, string Url, int SortOrder = 0, Guid? StoreTabId = null);
public sealed record UpdateLinkRequest(string Type, string Label, string? Icon, string Url, int SortOrder = 0, Guid? StoreTabId = null, bool? IsActive = null);
public sealed record LinkResponse(Guid Id, Guid StoreId, Guid? StoreTabId, string Type, string Label, string? Icon, string Url, int SortOrder, bool IsActive, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc)
{
    public static LinkResponse From(Link link) => new(link.Id, link.StoreId, link.StoreTabId, link.Type, link.Label, link.Icon, link.Url, link.SortOrder, link.IsActive, link.CreatedAtUtc, link.UpdatedAtUtc);
}

public interface ILinkService
{
    Task<IReadOnlyList<LinkResponse>> ListAsync(Guid storeId, CancellationToken cancellationToken);
    Task<LinkResponse> GetAsync(Guid storeId, Guid linkId, CancellationToken cancellationToken);
    Task<LinkResponse> CreateAsync(Guid storeId, CreateLinkRequest request, CancellationToken cancellationToken);
    Task<LinkResponse> UpdateAsync(Guid storeId, Guid linkId, UpdateLinkRequest request, CancellationToken cancellationToken);
    Task DeactivateAsync(Guid storeId, Guid linkId, CancellationToken cancellationToken);
}
