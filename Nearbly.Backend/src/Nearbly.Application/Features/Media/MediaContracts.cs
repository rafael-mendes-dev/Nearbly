using Nearbly.Domain.Entities;
using Nearbly.Application.Common;

namespace Nearbly.Application.Features.Media;

public sealed record MediaResponse(Guid Id, string Url, string MimeType, long SizeBytes, int Width, int Height, bool IsActive, DateTimeOffset CreatedAtUtc)
{
    public static MediaResponse From(MediaAsset media) => new(media.Id, $"/media/{media.Id}", media.MimeType, media.SizeBytes, media.Width, media.Height, media.IsActive, media.CreatedAtUtc);
}

public interface IMediaService
{
    Task<MediaResponse> UploadAsync(Guid storeId, MediaUpload upload, CancellationToken cancellationToken);
    Task DeactivateAsync(Guid storeId, Guid mediaId, CancellationToken cancellationToken);
    Task<StoredObject?> OpenReadAsync(Guid mediaId, CancellationToken cancellationToken);
}
