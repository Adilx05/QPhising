using AutoMapper;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.ArchiveTemplate;

public sealed class ArchiveTemplateMappingProfile : Profile
{
    public ArchiveTemplateMappingProfile()
    {
        CreateMap<Template, ArchiveTemplateResponse>();
    }
}
