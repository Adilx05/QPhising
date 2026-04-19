using FluentValidation;
using QPhising.Domain.Tracking.Aggregates;
using QPhising.Domain.Tracking.Models;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class CreateTrackingPageCommandValidator : AbstractValidator<CreateTrackingPageCommand>
{
    public CreateTrackingPageCommandValidator()
    {
        RuleFor(command => command.Slug)
            .NotEmpty()
            .MaximumLength(TrackingPageSlug.MaxLength);

        RuleFor(command => command.Title)
            .NotEmpty()
            .MaximumLength(TrackingPageAggregate.MaxTitleLength);

        RuleFor(command => command.Description)
            .MaximumLength(TrackingPageAggregate.MaxDescriptionLength)
            .When(command => !string.IsNullOrWhiteSpace(command.Description));

        RuleFor(command => command.OwnerId)
            .MaximumLength(TrackingPageAggregate.MaxOwnerIdLength)
            .When(command => !string.IsNullOrWhiteSpace(command.OwnerId));

        RuleFor(command => command.TemplateId)
            .Must(templateId => !templateId.HasValue || templateId.Value != Guid.Empty)
            .WithMessage("Template ID must be a non-empty GUID when provided.");

        RuleFor(command => command.CustomHtmlContent)
            .MaximumLength(TrackingPageAggregate.MaxCustomHtmlContentLength)
            .When(command => !string.IsNullOrWhiteSpace(command.CustomHtmlContent));

        RuleFor(command => command)
            .Must(command => !command.ValidFromUtc.HasValue || !command.ValidUntilUtc.HasValue || command.ValidUntilUtc.Value >= command.ValidFromUtc.Value)
            .WithMessage("Validity end date must be greater than or equal to start date.");

        RuleFor(command => command.RetentionDays)
            .InclusiveBetween(PageSettings.MinRetentionDays, PageSettings.MaxRetentionDays)
            .When(command => command.RetentionDays.HasValue);
    }
}
