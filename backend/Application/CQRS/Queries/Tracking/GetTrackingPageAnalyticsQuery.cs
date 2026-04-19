using MediatR;
using QPhising.Application.Contracts.Responses.Tracking;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed record GetTrackingPageAnalyticsQuery(
    Guid TrackingPageId,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    int TrendBucketSizeMinutes = 60,
    int RecentVisitLimit = 25) : IRequest<TrackingPageAnalyticsResult>;
