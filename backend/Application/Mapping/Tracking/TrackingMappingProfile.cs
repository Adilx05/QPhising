using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Domain.Tracking.Aggregates;
using QPhising.Domain.Tracking.Entities;

namespace QPhising.Application.Mapping.Tracking;

public static class TrackingMappingProfile
{
    public static TrackingPageResult ToResult(this TrackingPageAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        return new TrackingPageResult(
            Id: aggregate.Id,
            Slug: aggregate.Slug.Value,
            Title: aggregate.Title,
            Description: aggregate.Description,
            DestinationUrl: aggregate.DestinationUrl.Value,
            OwnerId: aggregate.OwnerId,
            PublishState: aggregate.PublishState,
            Settings: aggregate.Settings is null
                ? null
                : new TrackingPageSettingsResult(
                    aggregate.Settings.RetentionDays,
                    aggregate.Settings.MaskIpAddress,
                    aggregate.Settings.EnableBotFiltering,
                    aggregate.Settings.CaptureUtmParameters),
            CreatedAtUtc: aggregate.CreatedAtUtc,
            UpdatedAtUtc: aggregate.UpdatedAtUtc);
    }

    public static TrackingVisitTrendPointResult ToResult(this TrackingVisitTrendBucket bucket)
    {
        return new TrackingVisitTrendPointResult(bucket.BucketStartUtc, bucket.TotalVisits, bucket.UniqueVisitors);
    }

    public static TrackingRecentVisitResult ToResult(this VisitEventEntity visitEvent)
    {
        ArgumentNullException.ThrowIfNull(visitEvent);

        return new TrackingRecentVisitResult(
            Id: visitEvent.Id,
            OccurredAtUtc: visitEvent.OccurredAtUtc,
            SessionId: visitEvent.SessionId.Value,
            VisitorFingerprint: visitEvent.VisitorFingerprint.Value,
            UserAgent: visitEvent.UserAgent,
            ReferrerUrl: visitEvent.ReferrerUrl,
            IpHash: visitEvent.IpHash,
            IpAddressHashPolicy: visitEvent.IpAddressHashPolicy);
    }

    public static TrackingTopPageResult ToResult(this TrackingTopPageMetric metric)
    {
        ArgumentNullException.ThrowIfNull(metric);

        return new TrackingTopPageResult(
            TrackingPageId: metric.TrackingPageId,
            Slug: metric.Slug,
            Title: metric.Title,
            TotalVisits: metric.TotalVisits,
            UniqueVisitors: metric.UniqueVisitors);
    }

    public static TrackingRecentVisitStreamItemResult ToStreamResult(this TrackingRecentVisitMetric metric)
    {
        ArgumentNullException.ThrowIfNull(metric);

        return new TrackingRecentVisitStreamItemResult(
            VisitId: metric.VisitId,
            TrackingPageId: metric.TrackingPageId,
            TrackingPageSlug: metric.TrackingPageSlug,
            OccurredAtUtc: metric.OccurredAtUtc,
            SessionId: metric.SessionId,
            VisitorFingerprint: metric.VisitorFingerprint,
            UserAgent: metric.UserAgent,
            ReferrerUrl: metric.ReferrerUrl,
            IpHash: metric.IpHash,
            IpAddressHashPolicy: metric.IpAddressHashPolicy);
    }
}
