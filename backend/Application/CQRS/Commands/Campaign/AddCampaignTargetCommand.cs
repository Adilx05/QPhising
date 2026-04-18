using QPhising.Application.Contracts.Abstractions.Persistence;
using QPhising.Application.Contracts.Responses.Campaign;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed record AddCampaignTargetCommand(Guid CampaignId, string EmailAddress) : ITransactionalRequest<CampaignResult>;
