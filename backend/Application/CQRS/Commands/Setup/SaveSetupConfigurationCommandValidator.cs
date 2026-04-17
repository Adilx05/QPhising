using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed class SaveSetupConfigurationCommandValidator : AbstractValidator<SaveSetupConfigurationCommand>
{
    public SaveSetupConfigurationCommandValidator()
    {
        RuleFor(command => command.DatabaseConnectionString)
            .NotEmpty()
            .WithMessage("Database connection string is required.");

        RuleFor(command => command.RedisConnectionString)
            .NotEmpty()
            .WithMessage("Redis connection string is required.");

        RuleFor(command => command.KeycloakAuthority)
            .NotEmpty()
            .WithMessage("Keycloak authority is required.")
            .Must(authority => Uri.TryCreate(authority, UriKind.Absolute, out _))
            .WithMessage("Keycloak authority must be an absolute URI.");

        RuleFor(command => command.KeycloakRealm)
            .NotEmpty()
            .WithMessage("Keycloak realm is required.");

        RuleFor(command => command.KeycloakClientId)
            .NotEmpty()
            .WithMessage("Keycloak client ID is required.");

        RuleFor(command => command.KeycloakClientSecret)
            .NotEmpty()
            .WithMessage("Keycloak client secret is required.");
    }
}
