using AutoMapper;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.ScheduleCampaign;

public sealed class ScheduleCampaignMappingProfile : Profile
{
    public ScheduleCampaignMappingProfile()
    {
        CreateMap<Campaign, ScheduleCampaignResponse>();
    }
}
