using FluentValidation;

namespace QPhising.Application.Features.Setup.FinalizeSetup;

public sealed class FinalizeSetupCommandValidator : AbstractValidator<FinalizeSetupCommand>
{
    public FinalizeSetupCommandValidator()
    {
    }
}
