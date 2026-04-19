namespace QPhising.Application.Contracts.Abstractions.Tracking;

public sealed record TrackingTopPageMetric(
    Guid TrackingPageId,
    string Slug,
    string Title,
    int TotalVisits,
    int UniqueVisitors);
