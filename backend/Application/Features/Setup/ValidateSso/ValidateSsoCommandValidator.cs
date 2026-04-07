using FluentValidation;

namespace QPhising.Application.Features.Setup.ValidateSso;

public sealed class ValidateSsoCommandValidator : AbstractValidator<ValidateSsoCommand>
{
    public ValidateSsoCommandValidator()
    {
    }
}
