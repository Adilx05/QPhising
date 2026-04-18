using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Campaign;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed record PauseCampaignCommand(Guid CampaignId) : ITransactionalRequest<CampaignResult>;
