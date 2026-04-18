using FluentValidation;
using QPhising.Domain.Campaign.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Campaign name is required.")
            .MaximumLength(CampaignName.MaxLength)
            .WithMessage($"Campaign name must be at most {CampaignName.MaxLength} characters.");

        RuleFor(command => command.TemplateId)
            .NotEqual(Guid.Empty)
            .WithMessage("Template ID is required.");
    }
}
