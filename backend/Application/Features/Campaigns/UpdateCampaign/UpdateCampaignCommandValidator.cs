using FluentValidation;

namespace QPhising.Application.Features.Campaigns.UpdateCampaign;

public sealed class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.HtmlContent)
            .NotEmpty();

        RuleFor(command => command.TemplateType)
            .IsInEnum();

        RuleFor(command => command.StartDate)
            .LessThanOrEqualTo(command => command.EndDate)
            .WithMessage("Campaign start date must be less than or equal to end date.");
    }
}
