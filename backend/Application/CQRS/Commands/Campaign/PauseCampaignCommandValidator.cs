using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class PauseCampaignCommandValidator : AbstractValidator<PauseCampaignCommand>
{
    public PauseCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEqual(Guid.Empty)
            .WithMessage("Campaign ID is required.");
    }
}
