namespace Nearbly.Domain.Entities;

public sealed class Link
{
    private Link() { }

    public Link(Guid storeId, string type, string label, string? icon, string url, int sortOrder = 0, Guid? storeTabId = null, DateTimeOffset? nowUtc = null)
    {
        Id = Guid.NewGuid();
        StoreId = storeId;
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        Update(type, label, icon, url, sortOrder, storeTabId, now);
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid? StoreTabId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Label { get; private set; } = string.Empty;
    public string? Icon { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Store Store { get; private set; } = null!;
    public StoreTab? StoreTab { get; private set; }

    public void Update(string type, string label, string? icon, string url, int sortOrder, Guid? storeTabId, DateTimeOffset? nowUtc = null)
    {
        if (string.IsNullOrWhiteSpace(type) || type.Trim().Length > 80)
            throw new ArgumentException("Type is required and must have at most 80 characters.", nameof(type));
        if (string.IsNullOrWhiteSpace(label) || label.Trim().Length > 160)
            throw new ArgumentException("Label is required and must have at most 160 characters.", nameof(label));
        if (!UrlValidator.IsValid(url))
            throw new ArgumentException("URL must be an absolute HTTP or HTTPS URL without credentials.", nameof(url));
        if (sortOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(sortOrder));
        Type = type.Trim().ToLowerInvariant();
        Label = label.Trim();
        Icon = string.IsNullOrWhiteSpace(icon) ? null : icon.Trim();
        Url = url.Trim();
        SortOrder = sortOrder;
        StoreTabId = storeTabId;
        UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
