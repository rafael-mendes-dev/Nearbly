namespace Nearbly.Domain.Entities;

public sealed class MarkdownBlock
{
    private MarkdownBlock() { }

    public MarkdownBlock(Guid storeId, Guid storeTabId, string? title, string markdown, int sortOrder = 0, DateTimeOffset? nowUtc = null)
    {
        Id = Guid.NewGuid();
        StoreId = storeId;
        StoreTabId = storeTabId;
        var now = nowUtc ?? DateTimeOffset.UtcNow;
        Update(title, markdown, sortOrder, now);
        CreatedAtUtc = now;
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid StoreTabId { get; private set; }
    public string? Title { get; private set; }
    public string Markdown { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public Store Store { get; private set; } = null!;
    public StoreTab StoreTab { get; private set; } = null!;

    public void Update(string? title, string markdown, int sortOrder, DateTimeOffset? nowUtc = null)
    {
        if (title is not null && title.Trim().Length > 160) throw new ArgumentException("Title must have at most 160 characters.", nameof(title));
        if (string.IsNullOrWhiteSpace(markdown) || markdown.Trim().Length > 20_000) throw new ArgumentException("Markdown is required and must have at most 20000 characters.", nameof(markdown));
        if (sortOrder < 0) throw new ArgumentOutOfRangeException(nameof(sortOrder));
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        Markdown = markdown.Trim();
        SortOrder = sortOrder;
        UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow;
    }

    public void Deactivate(DateTimeOffset? nowUtc = null) { IsActive = false; UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow; }
    public void Activate(DateTimeOffset? nowUtc = null) { IsActive = true; UpdatedAtUtc = nowUtc ?? DateTimeOffset.UtcNow; }
}
