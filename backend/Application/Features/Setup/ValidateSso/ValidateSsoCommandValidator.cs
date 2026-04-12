using FluentValidation;

namespace QPhising.Application.Features.Setup.ValidateSso;

public sealed class ValidateSsoCommandValidator : AbstractValidator<ValidateSsoCommand>
{
    public ValidateSsoCommandValidator()
    {
        RuleFor(command => command.Authority)
            .NotEmpty()
            .Must(BeValidUri)
            .WithMessage("Authority must be a valid absolute URL.");

        RuleFor(command => command.Realm).NotEmpty();
        RuleFor(command => command.ClientId).NotEmpty();
        RuleFor(command => command.ClientSecret).NotEmpty();
        RuleFor(command => command.Audience).NotEmpty();
    }

    private static bool BeValidUri(string authority)
    {
        return Uri.TryCreate(authority, UriKind.Absolute, out Uri? parsed)
            && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
    }
}
