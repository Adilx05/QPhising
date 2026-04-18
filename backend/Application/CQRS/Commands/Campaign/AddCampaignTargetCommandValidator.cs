using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class AddCampaignTargetCommandValidator : AbstractValidator<AddCampaignTargetCommand>
{
    public AddCampaignTargetCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEqual(Guid.Empty)
            .WithMessage("Campaign ID is required.");

        RuleFor(command => command.EmailAddress)
            .NotEmpty()
            .WithMessage("Target email address is required.")
            .EmailAddress()
            .WithMessage("Target email address is invalid.");
    }
}
