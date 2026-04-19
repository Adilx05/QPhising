namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingAnalyticsOverviewResult(
    TrackingAnalyticsSummaryResult Summary,
    IReadOnlyCollection<TrackingTopPageResult> TopPages,
    IReadOnlyCollection<TrackingRecentVisitStreamItemResult> RecentVisits,
    IReadOnlyCollection<TrackingVisitTrendPointResult> Trends,
    IReadOnlyCollection<TrackingMetricDefinitionResult> MetricDefinitions);
