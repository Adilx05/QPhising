using FluentValidation;
using QPhising.Domain.Campaign.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEqual(Guid.Empty)
            .WithMessage("Campaign ID is required.");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Campaign name is required.")
            .MaximumLength(CampaignName.MaxLength)
            .WithMessage($"Campaign name must be at most {CampaignName.MaxLength} characters.");
    }
}
