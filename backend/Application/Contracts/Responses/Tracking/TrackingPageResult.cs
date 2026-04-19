using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.Contracts.Responses.Tracking;

public sealed record TrackingPageResult(
    Guid Id,
    string Slug,
    string Title,
    string? Description,
    string OwnerId,
    Guid? TemplateId,
    string? CustomHtmlContent,
    DateTimeOffset? ValidFromUtc,
    DateTimeOffset? ValidUntilUtc,
    TrackingPagePublishState PublishState,
    TrackingPageSettingsResult? Settings,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
