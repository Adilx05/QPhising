using AutoMapper;
using QPhising.Application.Features.Templates.GetTemplateById;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.ListTemplates;

public sealed class TemplateReadMappingProfile : Profile
{
    public TemplateReadMappingProfile()
    {
        CreateMap<Template, TemplateListItemResponse>()
            .ForCtorParam(nameof(TemplateListItemResponse.Variables),
                configuration => configuration.MapFrom(template =>
                    template.Variables.Select(variable => variable.Name).ToArray()));

        CreateMap<Template, TemplateDetailResponse>()
            .ForCtorParam(nameof(TemplateDetailResponse.Variables),
                configuration => configuration.MapFrom(template =>
                    template.Variables.Select(variable => variable.Name).ToArray()));
    }
}
