namespace Nearbly.Domain.Entities;

public sealed class LinkClick
{
    private LinkClick() { }

    public LinkClick(Guid storeId, Guid linkId, TrafficSource source, DateTimeOffset occurredAtUtc)
    {
        Id = Guid.NewGuid();
        StoreId = storeId;
        LinkId = linkId;
        Source = source;
        OccurredAtUtc = occurredAtUtc.ToUniversalTime();
    }

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid LinkId { get; private set; }
    public TrafficSource Source { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public Store Store { get; private set; } = null!;
    public Link Link { get; private set; } = null!;
}
