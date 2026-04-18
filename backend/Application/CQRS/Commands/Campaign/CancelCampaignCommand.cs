using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Campaign;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed record CancelCampaignCommand(Guid CampaignId) : ITransactionalRequest<CampaignResult>;
