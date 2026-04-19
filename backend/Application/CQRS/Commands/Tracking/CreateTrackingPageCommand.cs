using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Tracking;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed record CreateTrackingPageCommand(
    string Slug,
    string Title,
    string? Description,
    string? OwnerId,
    Guid? TemplateId,
    string? CustomHtmlContent,
    DateTimeOffset? ValidFromUtc,
    DateTimeOffset? ValidUntilUtc,
    int? RetentionDays,
    bool? MaskIpAddress,
    bool? EnableBotFiltering,
    bool? CaptureUtmParameters) : ITransactionalRequest<TrackingPageResult>;
