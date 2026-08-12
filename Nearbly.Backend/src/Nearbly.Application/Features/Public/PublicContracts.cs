using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Public;

public sealed record PublicLinkResponse(Guid Id, string Type, string Label, string? Icon, string Href);
public sealed record PublicTabResponse(Guid Id, string Key, string Name, int SortOrder, IReadOnlyList<PublicLinkResponse> Links);
public sealed record PublicStoreResponse(Guid Id, string Name, string Slug, string? Description, string? LogoUrl, string? PrimaryColor, string? SecondaryColor, IReadOnlyList<PublicLinkResponse> Links, IReadOnlyList<PublicTabResponse> Tabs);
public sealed record RegisterPageViewRequest(TrafficSource? Source = null);

public interface IPublicService
{
    Task<PublicStoreResponse?> GetStoreAsync(string slug, CancellationToken cancellationToken);
    Task RegisterViewAsync(string slug, RegisterPageViewRequest request, CancellationToken cancellationToken);
    Task<Uri> RegisterClickAsync(Guid linkId, TrafficSource source, CancellationToken cancellationToken);
}
