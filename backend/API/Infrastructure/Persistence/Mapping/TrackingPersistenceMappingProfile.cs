using QPhising.Api.Infrastructure.Persistence.Entities;
using QPhising.Domain.Tracking.Aggregates;
using QPhising.Domain.Tracking.Enums;
using QPhising.Domain.Tracking.Models;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Api.Infrastructure.Persistence.Mapping;

public static class TrackingPersistenceMappingProfile
{
    public static TrackingPageAggregate ToAggregate(this TrackingPageEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var settings = entity.RetentionDays.HasValue
            ? new PageSettings(
                entity.RetentionDays.Value,
                entity.CaptureIpAddress ?? true,
                (IpAddressHashPolicy)(entity.IpAddressHashPolicy ?? (int)IpAddressHashPolicy.Sha256),
                entity.EnableBotFiltering ?? true,
                entity.CaptureUtmParameters ?? true)
            : null;

        return TrackingPageAggregate.Rehydrate(
            entity.Id,
            new TrackingPageSlug(entity.Slug),
            entity.Title,
            entity.Description,
            entity.OwnerId,
            entity.TemplateId,
            entity.CustomHtmlContent,
            entity.ValidFromUtc,
            entity.ValidUntilUtc,
            (TrackingPagePublishState)entity.PublishState,
            settings,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc);
    }

    public static TrackingPageEntity ToEntity(this TrackingPageAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);

        return new TrackingPageEntity
        {
            Id = aggregate.Id,
            Slug = aggregate.Slug.Value,
            Title = aggregate.Title,
            Description = aggregate.Description,
            OwnerId = aggregate.OwnerId,
            TemplateId = aggregate.TemplateId,
            CustomHtmlContent = aggregate.CustomHtmlContent,
            ValidFromUtc = aggregate.ValidFromUtc,
            ValidUntilUtc = aggregate.ValidUntilUtc,
            PublishState = (int)aggregate.PublishState,
            RetentionDays = aggregate.Settings?.RetentionDays,
            CaptureIpAddress = aggregate.Settings?.CaptureIpAddress,
            IpAddressHashPolicy = aggregate.Settings is null ? null : (int)aggregate.Settings.IpAddressHashPolicy,
            EnableBotFiltering = aggregate.Settings?.EnableBotFiltering,
            CaptureUtmParameters = aggregate.Settings?.CaptureUtmParameters,
            CreatedAtUtc = aggregate.CreatedAtUtc,
            UpdatedAtUtc = aggregate.UpdatedAtUtc
        };
    }

    public static QPhising.Domain.Tracking.Entities.VisitEventEntity ToDomainEntity(this VisitEventEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new QPhising.Domain.Tracking.Entities.VisitEventEntity(
            entity.Id,
            entity.TrackingPageId,
            entity.OccurredAtUtc,
            new TrackingIdentifier(entity.SessionId),
            new TrackingIdentifier(entity.VisitorFingerprint),
            entity.UserAgent,
            entity.ReferrerUrl,
            entity.IpHash,
            (IpAddressHashPolicy)entity.IpAddressHashPolicy);
    }

    public static VisitEventEntity ToPersistenceEntity(this QPhising.Domain.Tracking.Entities.VisitEventEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new VisitEventEntity
        {
            Id = entity.Id,
            TrackingPageId = entity.TrackingPageId,
            OccurredAtUtc = entity.OccurredAtUtc,
            SessionId = entity.SessionId.Value,
            VisitorFingerprint = entity.VisitorFingerprint.Value,
            UserAgent = entity.UserAgent,
            ReferrerUrl = entity.ReferrerUrl,
            IpHash = entity.IpHash,
            IpAddressHashPolicy = (int)entity.IpAddressHashPolicy
        };
    }
}
