using AutoMapper;
using QPhising.Application.Features.Campaigns.GetCampaignById;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.ListCampaigns;

public sealed class CampaignReadMappingProfile : Profile
{
    public CampaignReadMappingProfile()
    {
        CreateMap<Campaign, CampaignListItemResponse>();
        CreateMap<Campaign, CampaignDetailResponse>();
    }
}
