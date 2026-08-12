namespace Nearbly.Domain.Entities;

public sealed class GalleryItem
{
    private GalleryItem() { }

    public GalleryItem(Guid storeId, Guid storeTabId, Guid mediaAssetId, string altText, string? caption, int sortOrder = 0, DateTimeOffset? nowUtc = null)
    {
        Id = Guid.NewGuid();
        StoreId = storeId;
        StoreTabId = storeTabId;
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        Update(mediaAssetId, altText, caption, sortOrder, now);
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid StoreTabId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public string AltText { get; private set; } = string.Empty;
    public string? Caption { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Store Store { get; private set; } = null!;
    public StoreTab StoreTab { get; private set; } = null!;
    public MediaAsset MediaAsset { get; private set; } = null!;

    public void Update(Guid mediaAssetId, string altText, string? caption, int sortOrder, DateTimeOffset? nowUtc = null)
    {
        if (mediaAssetId == Guid.Empty) throw new ArgumentException("Image is required.", nameof(mediaAssetId));
        if (string.IsNullOrWhiteSpace(altText) || altText.Trim().Length > 200) throw new ArgumentException("AltText is required and must have at most 200 characters.", nameof(altText));
        if (caption is not null && caption.Trim().Length > 500) throw new ArgumentException("Caption must have at most 500 characters.", nameof(caption));
        if (sortOrder < 0) throw new ArgumentOutOfRangeException(nameof(sortOrder));
        MediaAssetId = mediaAssetId;
        AltText = altText.Trim();
        Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim();
        SortOrder = sortOrder;
        UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow;
    }

    public void Deactivate(DateTimeOffset? nowUtc = null) { IsActive = false; UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow; }
    public void Activate(DateTimeOffset? nowUtc = null) { IsActive = true; UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow; }
}
