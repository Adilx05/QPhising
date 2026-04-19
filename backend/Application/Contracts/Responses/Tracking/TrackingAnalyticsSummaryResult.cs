namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingAnalyticsSummaryResult(
    int TotalVisits,
    int UniqueVisitors,
    DateTimeOffset? LastVisitAtUtc);
