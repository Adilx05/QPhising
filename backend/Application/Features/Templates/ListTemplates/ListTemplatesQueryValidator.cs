using FluentValidation;

namespace QPhising.Application.Features.Templates.ListTemplates;

public sealed class ListTemplatesQueryValidator : AbstractValidator<ListTemplatesQuery>
{
    public ListTemplatesQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThan(0);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(query => query.SearchTerm)
            .MaximumLength(200)
            .When(query => !string.IsNullOrWhiteSpace(query.SearchTerm));
    }
}
