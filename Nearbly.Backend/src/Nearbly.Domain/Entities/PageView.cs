namespace Nearbly.Domain.Entities;

public sealed class PageView
{
    private PageView() { }

    public PageView(Guid storeId, TrafficSource source, DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        StoreId = storeId;
        Source = source;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public TrafficSource Source { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public Store Store { get; private set; } = null!;
}
