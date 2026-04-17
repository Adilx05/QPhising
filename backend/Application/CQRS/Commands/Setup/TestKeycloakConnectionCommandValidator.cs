using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Setup;

public sealed class TestKeycloakConnectionCommandValidator : AbstractValidator<TestKeycloakConnectionCommand>
{
    public TestKeycloakConnectionCommandValidator()
    {
        RuleFor(command => command.Authority)
            .NotEmpty()
            .WithMessage("Keycloak authority is required.")
            .Must(authority => Uri.TryCreate(authority, UriKind.Absolute, out _))
            .WithMessage("Keycloak authority must be an absolute URI.");

        RuleFor(command => command.Realm)
            .NotEmpty()
            .WithMessage("Keycloak realm is required.");

        RuleFor(command => command.ClientId)
            .NotEmpty()
            .WithMessage("Keycloak client ID is required.");

        RuleFor(command => command.ClientSecret)
            .NotEmpty()
            .WithMessage("Keycloak client secret is required.");
    }
}
