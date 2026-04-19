using FluentValidation;
using QPhising.Domain.Tracking.Aggregates;
using QPhising.Domain.Tracking.Models;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class UpdateTrackingPageCommandValidator : AbstractValidator<UpdateTrackingPageCommand>
{
    public UpdateTrackingPageCommandValidator()
    {
        RuleFor(command => command.TrackingPageId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tracking page ID is required.");

        RuleFor(command => command.Slug)
            .NotEmpty()
            .MaximumLength(TrackingPageSlug.MaxLength);

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(TrackingPageAggregate.MaxTitleLength);

        RuleFor(command => command.Description)
            .MaximumLength(TrackingPageAggregate.MaxDescriptionLength)
            .When(command => !string.IsNullOrWhiteSpace(command.Description));

        RuleFor(command => command.DestinationUrl)
            .NotEmpty()
            .MaximumLength(TrackingPageUrl.MaxLength);


        RuleFor(command => command.TemplateId)
            .Must(templateId => !templateId.HasValue || templateId.Value != Guid.Empty)
            .WithMessage("Template ID must be a non-empty GUID when provided.");

        RuleFor(command => command.RetentionDays)
            .InclusiveBetween(PageSettings.MinRetentionDays, PageSettings.MaxRetentionDays)
            .When(command => command.RetentionDays.HasValue);
    }
}
