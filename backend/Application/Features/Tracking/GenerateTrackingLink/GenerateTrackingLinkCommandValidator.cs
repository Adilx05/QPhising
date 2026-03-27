using FluentValidation;

namespace QPhising.Application.Features.Tracking.GenerateTrackingLink;

public sealed class GenerateTrackingLinkCommandValidator : AbstractValidator<GenerateTrackingLinkCommand>
{
    public GenerateTrackingLinkCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEmpty();

        RuleFor(command => command.RecipientEmail)
            .NotEmpty()
            .EmailAddress();
    }
}
