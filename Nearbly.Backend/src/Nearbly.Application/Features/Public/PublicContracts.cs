using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Public;

public sealed record PublicLinkResponse(Guid Id, string Type, string Label, string? Icon, string Href);
public sealed record PublicProductResponse(Guid Id, string Name, string? Description, string ImageUrl, decimal? Price, bool IsAvailable, int SortOrder);
public sealed record PublicMarkdownBlockResponse(Guid Id, string? Title, string Markdown, int SortOrder);
public sealed record PublicGalleryItemResponse(Guid Id, string ImageUrl, string AltText, string? Caption, int SortOrder);
public sealed record PublicTabResponse(Guid Id, string Key, string Name, string ContentType, int SortOrder, IReadOnlyList<PublicLinkResponse> Links, IReadOnlyList<PublicProductResponse> Products, IReadOnlyList<PublicMarkdownBlockResponse> MarkdownBlocks, IReadOnlyList<PublicGalleryItemResponse> GalleryItems);
public sealed record PublicStoreResponse(Guid Id, string Name, string Slug, string? Description, string? LogoUrl, string? PrimaryColor, string? SecondaryColor, IReadOnlyList<PublicLinkResponse> Links, IReadOnlyList<PublicTabResponse> Tabs);
public sealed record RegisterPageViewRequest(TrafficSource? Source = null);

public interface IPublicService
{
    Task<PublicStoreResponse?> GetStoreAsync(string slug, CancellationToken cancellationToken);
    Task RegisterViewAsync(string slug, RegisterPageViewRequest request, CancellationToken cancellationToken);
    Task<Uri> RegisterClickAsync(Guid linkId, TrafficSource source, CancellationToken cancellationToken);
}
