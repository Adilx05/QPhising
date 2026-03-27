using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;

namespace QPhising.Application.Features.Campaigns.ListCampaigns;

public sealed class ListCampaignsQueryHandler(
    ICampaignRepository campaignRepository,
    IMapper mapper) : IRequestHandler<ListCampaignsQuery, Result<ListCampaignsResponse>>
{
    public async Task<Result<ListCampaignsResponse>> Handle(ListCampaignsQuery request, CancellationToken cancellationToken)
    {
        CampaignReadCriteria criteria = new(
            request.Statuses,
            request.TemplateTypes,
            request.StartsOnOrAfter,
            request.EndsOnOrBefore,
            request.Skip,
            request.Take);

        var campaigns = await campaignRepository.ListAsync(criteria, cancellationToken);
        var items = mapper.Map<IReadOnlyCollection<CampaignListItemResponse>>(campaigns);

        ListCampaignsResponse response = new(request.Skip, request.Take, items);
        return Result<ListCampaignsResponse>.Success(response);
    }
}
