using FluentValidation;

namespace QPhising.Application.Features.Health;

public sealed class GetHealthQueryValidator : AbstractValidator<GetHealthQuery>
{
    public GetHealthQueryValidator()
    {
        RuleFor(query => query).NotNull();
    }
}
