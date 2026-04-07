using FluentValidation;

namespace QPhising.Application.Features.Setup.ValidateDatabase;

public sealed class ValidateDatabaseCommandValidator : AbstractValidator<ValidateDatabaseCommand>
{
    public ValidateDatabaseCommandValidator()
    {
    }
}
