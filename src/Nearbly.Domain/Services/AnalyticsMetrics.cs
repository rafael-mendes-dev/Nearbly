namespace Nearbly.Domain.Services;

public static class AnalyticsMetrics
{
    public static decimal CalculateCtr(long views, long clicks) => views <= 0 ? 0m : Math.Round(clicks * 100m / views, 2);
}
