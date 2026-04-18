using FluentValidation;

namespace QPhising.Application.CQRS.Commands.RuntimeConfiguration;

public sealed class UpdateRuntimeConfigurationCommandValidator : AbstractValidator<UpdateRuntimeConfigurationCommand>
{
    public UpdateRuntimeConfigurationCommandValidator()
    {
        RuleFor(command => command)
            .Must(HaveAtLeastOneUpdatedValue)
            .WithMessage("At least one runtime configuration value must be provided.");

        RuleFor(command => command.KeycloakAuthority)
            .Must(authority => string.IsNullOrWhiteSpace(authority) || Uri.TryCreate(authority, UriKind.Absolute, out _))
            .WithMessage("Keycloak authority must be an absolute URI when provided.");

        RuleFor(command => command)
            .Must(command =>
                string.IsNullOrWhiteSpace(command.KeycloakAuthority) ==
                string.IsNullOrWhiteSpace(command.KeycloakRealm))
            .WithMessage("Keycloak authority and realm must be provided together.");

        RuleFor(command => command)
            .Must(command =>
                string.IsNullOrWhiteSpace(command.KeycloakAuthority) ==
                string.IsNullOrWhiteSpace(command.KeycloakClientId))
            .WithMessage("Keycloak authority and client ID must be provided together.");

        RuleFor(command => command)
            .Must(command =>
                string.IsNullOrWhiteSpace(command.KeycloakAuthority) ==
                string.IsNullOrWhiteSpace(command.KeycloakClientSecret))
            .WithMessage("Keycloak authority and client secret must be provided together.");
    }

    private static bool HaveAtLeastOneUpdatedValue(UpdateRuntimeConfigurationCommand command) =>
        !string.IsNullOrWhiteSpace(command.DatabaseConnectionString) ||
        !string.IsNullOrWhiteSpace(command.RedisConnectionString) ||
        !string.IsNullOrWhiteSpace(command.KeycloakAuthority) ||
        !string.IsNullOrWhiteSpace(command.KeycloakRealm) ||
        !string.IsNullOrWhiteSpace(command.KeycloakClientId) ||
        !string.IsNullOrWhiteSpace(command.KeycloakClientSecret);
}
