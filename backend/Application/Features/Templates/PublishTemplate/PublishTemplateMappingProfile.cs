using AutoMapper;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.PublishTemplate;

public sealed class PublishTemplateMappingProfile : Profile
{
    public PublishTemplateMappingProfile()
    {
        CreateMap<Template, PublishTemplateResponse>();
    }
}
