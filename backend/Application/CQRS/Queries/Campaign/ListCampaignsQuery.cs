using MediatR;
using QPhising.Application.Contracts.Responses.Campaign;

namespace QPhising.Application.CQRS.Queries.Campaign;

public sealed record ListCampaignsQuery : IRequest<IReadOnlyCollection<CampaignResult>>;
