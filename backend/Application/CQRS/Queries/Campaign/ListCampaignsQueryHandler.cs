using MediatR;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Responses.Campaign;

namespace QPhising.Application.CQRS.Queries.Campaign;

public sealed class ListCampaignsQueryHandler : IRequestHandler<ListCampaignsQuery, IReadOnlyCollection<CampaignResult>>
{
    private readonly ICampaignRepository _campaignRepository;

    public ListCampaignsQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<IReadOnlyCollection<CampaignResult>> Handle(ListCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await _campaignRepository.ListAsync(cancellationToken);
        return campaigns.Select(CampaignResult.FromAggregate).ToArray();
    }
}
