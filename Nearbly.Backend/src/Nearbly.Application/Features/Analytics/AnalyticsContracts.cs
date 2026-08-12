using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Analytics;

public sealed record AnalyticsLinkResponse(Guid LinkId, string Label, string Type, long Clicks);
public sealed record AnalyticsDayResponse(DateOnly Date, long Views);
public sealed record StoreAnalyticsResponse(long Views, long Clicks, decimal Ctr, IReadOnlyDictionary<TrafficSource, long> Sources, IReadOnlyList<AnalyticsLinkResponse> TopLinks, IReadOnlyList<AnalyticsDayResponse> ViewsByDay);

public interface IAnalyticsService
{
    Task<StoreAnalyticsResponse> GetAsync(Guid storeId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
}
