using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class CancelCampaignCommandValidator : AbstractValidator<CancelCampaignCommand>
{
    public CancelCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEqual(Guid.Empty)
            .WithMessage("Campaign ID is required.");
    }
}
