namespace Nearbly.Domain.Entities;

public sealed class StoreTab
{
    private StoreTab() { }

    public StoreTab(Guid storeId, string key, string name, int sortOrder = 0, DateTimeOffset? nowUtc = null, ContentType contentType = ContentType.Links)
    {
        Id = Guid.NewGuid();
        StoreId = storeId;
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        Update(key, name, sortOrder, contentType, now);
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public ContentType ContentType { get; private set; } = ContentType.Links;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Store Store { get; private set; } = null!;
    public ICollection<Link> Links { get; private set; } = new List<Link>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();
    public ICollection<MarkdownBlock> MarkdownBlocks { get; private set; } = new List<MarkdownBlock>();
    public ICollection<GalleryItem> GalleryItems { get; private set; } = new List<GalleryItem>();

    public void Update(string key, string name, int sortOrder, DateTimeOffset? nowUtc = null) => Update(key, name, sortOrder, ContentType, nowUtc);

    public void Update(string key, string name, int sortOrder, ContentType contentType, DateTimeOffset? nowUtc = null)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Trim().Length > 80)
            throw new ArgumentException("Key is required and must have at most 80 characters.", nameof(key));
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120)
            throw new ArgumentException("Name is required and must have at most 120 characters.", nameof(name));
        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder));
        Key = key.Trim().ToLowerInvariant();
        Name = name.Trim();
        SortOrder = sortOrder;
        ContentType = contentType;
        UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow;
    }

    public void ChangeContentType(ContentType contentType, bool hasContent, DateTimeOffset? nowUtc = null)
    {
        if (hasContent && ContentType != contentType)
            throw new InvalidOperationException("A tab with content cannot change its content type.");
        ContentType = contentType;
        UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
