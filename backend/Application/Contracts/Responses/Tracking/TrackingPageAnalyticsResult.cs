namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingPageAnalyticsResult(
    Guid TrackingPageId,
    string Slug,
    TrackingAnalyticsSummaryResult Summary,
    IReadOnlyCollection<TrackingVisitTrendPointResult> Trends,
    IReadOnlyCollection<TrackingRecentVisitResult> RecentVisits);
