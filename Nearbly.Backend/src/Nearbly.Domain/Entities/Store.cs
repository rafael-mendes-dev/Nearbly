namespace Nearbly.Domain.Entities;

public sealed class Store
{
    private Store() { }

    public Store(string name, string slug, string? description = null, string? logoUrl = null, string? primaryColor = null, string? secondaryColor = null, DateTimeOffset? nowUtc = null)
    {
        Id = Guid.NewGuid();
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        Update(name, slug, description, logoUrl, primaryColor, secondaryColor, now);
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? LogoUrl { get; private set; }
    public Guid? LogoMediaId { get; private set; }
    public string? PrimaryColor { get; private set; }
    public string? SecondaryColor { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public ICollection<StoreTab> Tabs { get; private set; } = new List<StoreTab>();
    public ICollection<Link> Links { get; private set; } = new List<Link>();
    public MediaAsset? LogoMedia { get; private set; }

    public void Update(string name, string slug, string? description, string? logoUrl, string? primaryColor, string? secondaryColor, DateTimeOffset? nowUtc = null)
    {
        Name = RequiredText(name, 160, nameof(name));
        Slug = SlugNormalizer.Normalize(slug);
        Description = OptionalText(description, 500, nameof(description));
        LogoUrl = OptionalText(logoUrl, 2_048, nameof(logoUrl));
        if (!ColorValidator.IsValid(primaryColor) || !ColorValidator.IsValid(secondaryColor))
            throw new ArgumentException("Colors must use the #RRGGBB format.");
        PrimaryColor = string.IsNullOrWhiteSpace(primaryColor) ? null : primaryColor.Trim().ToUpperInvariant();
        SecondaryColor = string.IsNullOrWhiteSpace(secondaryColor) ? null : secondaryColor.Trim().ToUpperInvariant();
        UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;

    public void SetLogoMedia(Guid? mediaId, DateTimeOffset? nowUtc = null)
    {
        LogoMediaId = mediaId;
        UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow;
    }

    private static string RequiredText(string value, int maxLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > maxLength)
            throw new ArgumentException($"{name} is required and must have at most {maxLength} characters.", name);
        return value.Trim();
    }

    private static string? OptionalText(string? value, int maxLength, string name)
    {
        if (value is not null && value.Trim().Length > maxLength)
            throw new ArgumentException($"{name} must have at most {maxLength} characters.", name);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
