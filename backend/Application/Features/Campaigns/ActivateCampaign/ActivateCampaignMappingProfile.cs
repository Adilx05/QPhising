using AutoMapper;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.ActivateCampaign;

public sealed class ActivateCampaignMappingProfile : Profile
{
    public ActivateCampaignMappingProfile()
    {
        CreateMap<Campaign, ActivateCampaignResponse>();
    }
}
