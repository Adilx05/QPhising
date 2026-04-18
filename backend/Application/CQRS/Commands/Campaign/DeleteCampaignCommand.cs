using MediatR;
using QPhising.Application.Contracts.Abstractions.Persistence;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed record DeleteCampaignCommand(Guid CampaignId) : ITransactionalRequest<Unit>;
