using AutoMapper;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.UpdateCampaign;

public sealed class UpdateCampaignMappingProfile : Profile
{
    public UpdateCampaignMappingProfile()
    {
        CreateMap<Campaign, UpdateCampaignResponse>();
    }
}
