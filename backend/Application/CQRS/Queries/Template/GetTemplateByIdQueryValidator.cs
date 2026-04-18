using FluentValidation;

namespace QPhising.Application.CQRS.Queries.Template;

public sealed class GetTemplateByIdQueryValidator : AbstractValidator<GetTemplateByIdQuery>
{
    public GetTemplateByIdQueryValidator()
    {
        RuleFor(query => query.TemplateId)
            .NotEqual(Guid.Empty)
            .WithMessage("Template ID is required.");
    }
}
