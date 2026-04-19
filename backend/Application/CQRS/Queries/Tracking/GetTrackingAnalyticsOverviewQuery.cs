using MediatR;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;

namespace QPhising.Application.CQRS.Queries.Tracking;

public sealed record GetTrackingAnalyticsOverviewQuery(
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    TrackingVisitTrendBucketWindow TrendWindow = TrackingVisitTrendBucketWindow.Day,
    int TimezoneOffsetMinutes = 0,
    bool ExcludeBots = true,
    int TopPagesLimit = 10,
    int RecentVisitLimit = 25) : IRequest<TrackingAnalyticsOverviewResult>;
