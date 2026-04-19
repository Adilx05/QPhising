using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Campaign;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed record CreateCampaignCommand(
    string Name,
    string TrackingPageSlug,
    string TrackingPageTitle,
    string DestinationUrl,
    string? TrackingPageDescription,
    Guid? TemplateId) : ITransactionalRequest<CampaignResult>;
