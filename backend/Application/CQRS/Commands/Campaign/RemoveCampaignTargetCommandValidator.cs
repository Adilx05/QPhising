using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class RemoveCampaignTargetCommandValidator : AbstractValidator<RemoveCampaignTargetCommand>
{
    public RemoveCampaignTargetCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEqual(Guid.Empty)
            .WithMessage("Campaign ID is required.");

        RuleFor(command => command.TargetId)
            .NotEqual(Guid.Empty)
            .WithMessage("Target ID is required.");
    }
}
