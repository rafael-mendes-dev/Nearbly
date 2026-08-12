using Microsoft.EntityFrameworkCore;
using Nearbly.Application.Common;
using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Media;

public sealed class MediaService(INearblyDbContext db, IImageProcessor imageProcessor, IObjectStorage storage, TimeProvider timeProvider) : IMediaService
{
    public async Task<MediaResponse> UploadAsync(Guid storeId, MediaUpload upload, CancellationToken cancellationToken)
    {
        if (!await db.Stores.AnyAsync(x => x.Id == storeId, cancellationToken)) throw new NotFoundException("Store not found.");
        var processed = await imageProcessor.ProcessAsync(upload, cancellationToken);
        await using (processed.Content)
        {
            var asset = new MediaAsset(storeId, $"stores/{storeId:N}/{Guid.NewGuid():N}.webp", processed.MimeType, processed.Length, processed.Width, processed.Height, timeProvider.GetUtcNow());
            await storage.PutAsync(asset.StorageKey, processed.Content, asset.MimeType, cancellationToken);
            db.MediaAssets.Add(asset);
            await db.SaveChangesAsync(cancellationToken);
            return MediaResponse.From(asset);
        }
    }

    public async Task DeactivateAsync(Guid storeId, Guid mediaId, CancellationToken cancellationToken)
    {
        var media = await db.MediaAssets.SingleOrDefaultAsync(x => x.StoreId == storeId && x.Id == mediaId, cancellationToken)
            ?? throw new NotFoundException("Media not found.");
        var used = await db.Stores.AnyAsync(x => x.LogoMediaId == mediaId)
            || await db.Products.AnyAsync(x => x.MediaAssetId == mediaId)
            || await db.GalleryItems.AnyAsync(x => x.MediaAssetId == mediaId);
        if (used) throw new ConflictException("Referenced media cannot be deactivated.");
        media.Deactivate(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<StoredObject?> OpenReadAsync(Guid mediaId, CancellationToken cancellationToken)
    {
        var media = await db.MediaAssets.AsNoTracking().SingleOrDefaultAsync(x => x.Id == mediaId && x.IsActive, cancellationToken);
        return media is null ? null : await storage.OpenReadAsync(media.StorageKey, cancellationToken);
    }
}
