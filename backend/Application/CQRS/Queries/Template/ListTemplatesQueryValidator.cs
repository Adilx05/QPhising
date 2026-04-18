using FluentValidation;

namespace QPhising.Application.CQRS.Queries.Template;

public sealed class ListTemplatesQueryValidator : AbstractValidator<ListTemplatesQuery>
{
    public ListTemplatesQueryValidator()
    {
        RuleFor(query => query)
            .NotNull();
    }
}
