using Microsoft.EntityFrameworkCore;
using Nearbly.Application.Common;
using Nearbly.Domain.Entities;

namespace Nearbly.Application.Features.Analytics;

public sealed class AnalyticsService(INearblyDbContext db) : IAnalyticsService
{
    public async Task<StoreAnalyticsResponse> GetAsync(Guid storeId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        if (!await db.Stores.AsNoTracking().AnyAsync(x => x.Id == storeId, cancellationToken))
            throw new NotFoundException("Store not found.");
        if (from.HasValue && to.HasValue && from > to)
            throw new ArgumentException("The from date cannot be after the to date.");

        var start = from.HasValue ? from.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) : (DateTime?)null;
        var endExclusive = to.HasValue ? to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) : (DateTime?)null;
        var viewsQuery = db.PageViews.AsNoTracking().Where(x => x.StoreId == storeId);
        var clicksQuery = db.LinkClicks.AsNoTracking().Where(x => x.StoreId == storeId);
        if (start.HasValue) { viewsQuery = viewsQuery.Where(x => x.OccurredAtUtc >= start.Value); clicksQuery = clicksQuery.Where(x => x.OccurredAtUtc >= start.Value); }
        if (endExclusive.HasValue) { viewsQuery = viewsQuery.Where(x => x.OccurredAtUtc < endExclusive.Value); clicksQuery = clicksQuery.Where(x => x.OccurredAtUtc < endExclusive.Value); }

        var views = await viewsQuery.LongCountAsync(cancellationToken);
        var clicks = await clicksQuery.LongCountAsync(cancellationToken);
        var sources = await viewsQuery.GroupBy(x => x.Source).Select(group => new { group.Key, Count = group.LongCount() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        foreach (var source in Enum.GetValues<TrafficSource>()) sources.TryAdd(source, 0);

        var topLinks = await clicksQuery.GroupBy(x => new { x.LinkId, x.Link.Label, x.Link.Type }).OrderByDescending(x => x.LongCount()).ThenBy(x => x.Key.LinkId).Take(10).Select(x => new AnalyticsLinkResponse(x.Key.LinkId, x.Key.Label, x.Key.Type, x.LongCount())).ToListAsync(cancellationToken);
        var byDay = await viewsQuery.GroupBy(x => new { x.OccurredAtUtc.Year, x.OccurredAtUtc.Month, x.OccurredAtUtc.Day }).OrderBy(x => x.Key.Year).ThenBy(x => x.Key.Month).ThenBy(x => x.Key.Day).Select(x => new { x.Key.Year, x.Key.Month, x.Key.Day, Views = x.LongCount() }).ToListAsync(cancellationToken);
        var dayResponses = byDay.Select(x => new AnalyticsDayResponse(new DateOnly(x.Year, x.Month, x.Day), x.Views)).ToList();
        var ctr = AnalyticsMetrics.CalculateCtr(views, clicks);
        return new StoreAnalyticsResponse(views, clicks, ctr, sources, topLinks, dayResponses);
    }
}
