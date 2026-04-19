using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Domain.Tracking.Enums;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed record UpdateTrackingPageCommand(
    Guid TrackingPageId,
    string Slug,
    string Title,
    string? Description,
    Guid? TemplateId,
    string? CustomHtmlContent,
    DateTimeOffset? ValidFromUtc,
    DateTimeOffset? ValidUntilUtc,
    int? RetentionDays,
    bool? CaptureIpAddress,
    IpAddressHashPolicy? IpAddressHashPolicy,
    bool? EnableBotFiltering,
    bool? CaptureUtmParameters) : ITransactionalRequest<TrackingPageResult>;
