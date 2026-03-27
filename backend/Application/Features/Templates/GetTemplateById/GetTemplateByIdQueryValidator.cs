using FluentValidation;

namespace QPhising.Application.Features.Templates.GetTemplateById;

public sealed class GetTemplateByIdQueryValidator : AbstractValidator<GetTemplateByIdQuery>
{
    public GetTemplateByIdQueryValidator()
    {
        RuleFor(query => query.TemplateId)
            .NotEmpty();
    }
}
