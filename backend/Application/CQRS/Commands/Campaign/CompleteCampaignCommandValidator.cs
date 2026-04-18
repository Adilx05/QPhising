using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class CompleteCampaignCommandValidator : AbstractValidator<CompleteCampaignCommand>
{
    public CompleteCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEqual(Guid.Empty)
            .WithMessage("Campaign ID is required.");
    }
}
