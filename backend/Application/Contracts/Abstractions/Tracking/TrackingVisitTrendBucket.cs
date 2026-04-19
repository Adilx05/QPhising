namespace QPhising.Application.Contracts.Abstractions.Tracking;

public sealed record TrackingVisitTrendBucket(
    DateTimeOffset BucketStartUtc,
    int TotalVisits,
    int UniqueVisitors);
