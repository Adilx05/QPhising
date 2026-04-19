namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingVisitTrendPointResult(
    DateTimeOffset BucketStartUtc,
    int TotalVisits,
    int UniqueVisitors);
