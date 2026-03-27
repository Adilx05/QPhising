using AutoMapper;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.CreateTemplate;

public sealed class CreateTemplateMappingProfile : Profile
{
    public CreateTemplateMappingProfile()
    {
        CreateMap<Template, CreateTemplateResponse>()
            .ForCtorParam(nameof(CreateTemplateResponse.Variables),
                configuration => configuration.MapFrom(template =>
                    template.Variables.Select(variable => variable.Name).ToArray()));
    }
}
