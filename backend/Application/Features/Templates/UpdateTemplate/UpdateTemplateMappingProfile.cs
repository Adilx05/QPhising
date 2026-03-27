using AutoMapper;
using QPhising.Domain.Templates;

namespace QPhising.Application.Features.Templates.UpdateTemplate;

public sealed class UpdateTemplateMappingProfile : Profile
{
    public UpdateTemplateMappingProfile()
    {
        CreateMap<Template, UpdateTemplateResponse>()
            .ForCtorParam(nameof(UpdateTemplateResponse.Variables),
                configuration => configuration.MapFrom(template =>
                    template.Variables.Select(variable => variable.Name).ToArray()));
    }
}
