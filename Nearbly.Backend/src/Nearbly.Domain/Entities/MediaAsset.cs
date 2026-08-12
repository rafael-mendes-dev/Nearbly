namespace Nearbly.Domain.Entities;

public sealed class MediaAsset
{
    private MediaAsset() { }

    public MediaAsset(Guid storeId, string storageKey, string mimeType, long sizeBytes, int width, int height, DateTimeOffset? nowUtc = null)
    {
        if (storeId == Guid.Empty) throw new ArgumentException("Store is required.", nameof(storeId));
        if (string.IsNullOrWhiteSpace(storageKey)) throw new ArgumentException("Storage key is required.", nameof(storageKey));
        if (sizeBytes <= 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes));
        if (width <= 0 || height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        Id = Guid.NewGuid();
        StoreId = storeId;
        StorageKey = storageKey.Trim();
        MimeType = mimeType.Trim().ToLowerInvariant();
        SizeBytes = sizeBytes;
        Width = width;
        Height = height;
        CreatedAtUtc = UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string MimeType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Store Store { get; private set; } = null!;

    public void Deactivate(DateTimeOffset? nowUtc = null)
    {
        IsActive = false;
        UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow;
    }
}
