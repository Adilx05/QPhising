using FluentValidation;

namespace QPhising.Application.Features.Setup.ApplyMigrations;

public sealed class ApplyMigrationsCommandValidator : AbstractValidator<ApplyMigrationsCommand>
{
    public ApplyMigrationsCommandValidator()
    {
        RuleFor(command => command)
            .Must(HaveConnectionInformation)
            .WithMessage("Provide either a full connection string or host/port/database/user/password fields.");

        When(command => string.IsNullOrWhiteSpace(command.ConnectionString), () =>
        {
            RuleFor(command => command.Host).NotEmpty();
            RuleFor(command => command.Port).NotNull().InclusiveBetween(1, 65535);
            RuleFor(command => command.Database).NotEmpty();
            RuleFor(command => command.Username).NotEmpty();
            RuleFor(command => command.Password).NotEmpty();
        });
    }

    private static bool HaveConnectionInformation(ApplyMigrationsCommand command)
    {
        if (!string.IsNullOrWhiteSpace(command.ConnectionString))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(command.Host)
            && command.Port is >= 1 and <= 65535
            && !string.IsNullOrWhiteSpace(command.Database)
            && !string.IsNullOrWhiteSpace(command.Username)
            && !string.IsNullOrWhiteSpace(command.Password);
    }
}
