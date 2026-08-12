namespace Nearbly.Domain.Entities;

public sealed class Product
{
    private Product() { }

    public Product(Guid storeId, Guid storeTabId, string name, string? description, Guid mediaAssetId, decimal? price, bool isAvailable, int sortOrder = 0, DateTimeOffset? nowUtc = null)
    {
        Id = Guid.NewGuid();
        StoreId = storeId;
        StoreTabId = storeTabId;
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        Update(name, description, mediaAssetId, price, isAvailable, sortOrder, now);
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid StoreTabId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public decimal? Price { get; private set; }
    public bool IsAvailable { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Store Store { get; private set; } = null!;
    public StoreTab StoreTab { get; private set; } = null!;
    public MediaAsset MediaAsset { get; private set; } = null!;

    public void Update(string name, string? description, Guid mediaAssetId, decimal? price, bool isAvailable, int sortOrder, DateTimeOffset? nowUtc = null)
    {
        Name = RequiredText(name, 160, nameof(name));
        Description = OptionalText(description, 2_000, nameof(description));
        if (mediaAssetId == Guid.Empty) throw new ArgumentException("Image is required.", nameof(mediaAssetId));
        if (price is < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (sortOrder < 0) throw new ArgumentOutOfRangeException(nameof(sortOrder));
        MediaAssetId = mediaAssetId;
        Price = price;
        IsAvailable = isAvailable;
        SortOrder = sortOrder;
        UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow;
    }

    public void Deactivate(DateTimeOffset? nowUtc = null) { IsActive = false; UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow; }
    public void Activate(DateTimeOffset? nowUtc = null) { IsActive = true; UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow; }

    private static string RequiredText(string value, int maxLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > maxLength) throw new ArgumentException($"{name} is required and must have at most {maxLength} characters.", name);
        return value.Trim();
    }

    private static string? OptionalText(string? value, int maxLength, string name)
    {
        if (value is not null && value.Trim().Length > maxLength) throw new ArgumentException($"{name} must have at most {maxLength} characters.", name);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
