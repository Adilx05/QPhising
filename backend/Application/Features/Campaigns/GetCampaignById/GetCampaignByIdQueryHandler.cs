using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Domain.Abstractions;

namespace QPhising.Application.Features.Campaigns.GetCampaignById;

public sealed class GetCampaignByIdQueryHandler(
    ICampaignRepository campaignRepository,
    IMapper mapper) : IRequestHandler<GetCampaignByIdQuery, Result<CampaignDetailResponse>>
{
    public async Task<Result<CampaignDetailResponse>> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await campaignRepository.GetByIdAsync(request.CampaignId, cancellationToken);
        if (campaign is null)
        {
            return Result<CampaignDetailResponse>.Failure($"Campaign '{request.CampaignId}' was not found.");
        }

        CampaignDetailResponse response = mapper.Map<CampaignDetailResponse>(campaign);
        return Result<CampaignDetailResponse>.Success(response);
    }
}
