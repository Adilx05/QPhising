using FluentValidation;

namespace QPhising.Application.Features.Campaigns.ScheduleCampaign;

public sealed class ScheduleCampaignCommandValidator : AbstractValidator<ScheduleCampaignCommand>
{
    public ScheduleCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEmpty();
    }
}
