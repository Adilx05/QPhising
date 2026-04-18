using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Identity;

public sealed class ValidateAccessTokenCommandValidator : AbstractValidator<ValidateAccessTokenCommand>
{
    public ValidateAccessTokenCommandValidator()
    {
        RuleFor(command => command.AccessToken)
            .NotEmpty();
    }
}
