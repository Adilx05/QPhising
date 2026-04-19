using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed record IngestVisitEventCommand(
    Guid TrackingPageId,
    DateTimeOffset OccurredAtUtc,
    string SessionId,
    string VisitorFingerprint,
    string? UserAgent,
    string? ReferrerUrl,
    string? IpHash,
    IpAddressHashPolicy IpAddressHashPolicy,
    int DeduplicationWindowSeconds = 120) : ITransactionalRequest<VisitIngestionResult>;
