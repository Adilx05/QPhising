using AutoMapper;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Features.Campaigns.CreateCampaign;

public sealed class CreateCampaignMappingProfile : Profile
{
    public CreateCampaignMappingProfile()
    {
        CreateMap<Campaign, CreateCampaignResponse>();
    }
}
