using FluentValidation;
using QPhising.Domain.Campaign.ValueObjects;
using QPhising.Domain.Tracking.Aggregates;
using QPhising.Domain.Tracking.ValueObjects;

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

        RuleFor(command => command.TrackingPageSlug)
            .NotEmpty()
            .WithMessage("Tracking page slug is required.")
            .MaximumLength(TrackingPageSlug.MaxLength)
            .WithMessage($"Tracking page slug must be at most {TrackingPageSlug.MaxLength} characters.");

        RuleFor(command => command.TrackingPageTitle)
            .NotEmpty()
            .WithMessage("Tracking page title is required.")
            .MaximumLength(TrackingPageAggregate.MaxTitleLength)
            .WithMessage($"Tracking page title must be at most {TrackingPageAggregate.MaxTitleLength} characters.");

        RuleFor(command => command.HtmlContent)
            .MaximumLength(TrackingPageAggregate.MaxCustomHtmlContentLength)
            .When(command => !string.IsNullOrWhiteSpace(command.HtmlContent));

        RuleFor(command => command)
            .Must(command => !command.ValidFromUtc.HasValue || !command.ValidUntilUtc.HasValue || command.ValidUntilUtc.Value >= command.ValidFromUtc.Value)
            .WithMessage("Validity end date must be greater than or equal to start date.");
    }
}
