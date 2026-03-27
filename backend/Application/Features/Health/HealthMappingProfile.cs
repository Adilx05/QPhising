using AutoMapper;

namespace QPhising.Application.Features.Health;

public sealed class HealthMappingProfile : Profile
{
    public HealthMappingProfile()
    {
        CreateMap<HealthStatus, HealthStatusDto>();
    }
}
