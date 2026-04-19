namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record VisitIngestionResult(
    Guid VisitEventId,
    Guid TrackingPageId,
    bool IsDuplicate,
    DateTimeOffset RecordedAtUtc);
