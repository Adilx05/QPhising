using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Campaign;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed record CreateCampaignCommand(
    string Name,
    string TrackingPageSlug,
    string TrackingPageTitle,
    string? TrackingPageDescription,
    Guid? TemplateId,
    string? HtmlContent,
    DateTimeOffset? ValidFromUtc,
    DateTimeOffset? ValidUntilUtc) : ITransactionalRequest<CampaignResult>;
