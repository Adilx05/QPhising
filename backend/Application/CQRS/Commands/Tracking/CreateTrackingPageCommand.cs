using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Tracking;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed record CreateTrackingPageCommand(
    string Slug,
    string Title,
    string? Description,
    string DestinationUrl,
    string? OwnerId,
    Guid? TemplateId,
    int? RetentionDays,
    bool? MaskIpAddress,
    bool? EnableBotFiltering,
    bool? CaptureUtmParameters) : ITransactionalRequest<TrackingPageResult>;
