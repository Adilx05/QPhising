using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingPageResult(
    Guid Id,
    string Slug,
    string Title,
    string? Description,
    string DestinationUrl,
    string OwnerId,
    TrackingPagePublishState PublishState,
    TrackingPageSettingsResult? Settings,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
