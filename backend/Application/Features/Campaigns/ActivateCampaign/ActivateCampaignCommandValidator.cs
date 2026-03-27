using FluentValidation;

namespace QPhising.Application.Features.Campaigns.ActivateCampaign;

public sealed class ActivateCampaignCommandValidator : AbstractValidator<ActivateCampaignCommand>
{
    public ActivateCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEmpty();
    }
}
