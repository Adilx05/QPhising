using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class StartCampaignCommandValidator : AbstractValidator<StartCampaignCommand>
{
    public StartCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEqual(Guid.Empty)
            .WithMessage("Campaign ID is required.");
    }
}
