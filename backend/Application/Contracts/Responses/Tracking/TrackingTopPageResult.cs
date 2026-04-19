namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingTopPageResult(
    Guid TrackingPageId,
    string Slug,
    string Title,
    int TotalVisits,
    int UniqueVisitors);
