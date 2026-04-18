using MediatR;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Responses.Campaign;

namespace QPhising.Application.CQRS.Queries.Campaign;

public sealed class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, CampaignResult>
{
    private readonly ICampaignRepository _campaignRepository;

    public GetCampaignByIdQueryHandler(ICampaignRepository campaignRepository)
    {
        _campaignRepository = campaignRepository;
    }

    public async Task<CampaignResult> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var aggregate = await _campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        return CampaignResult.FromAggregate(aggregate);
    }
}
