using FluentValidation;
using QPhising.Domain.Tracking.Entities;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class IngestVisitEventCommandValidator : AbstractValidator<IngestVisitEventCommand>
{
    public IngestVisitEventCommandValidator()
    {
        RuleFor(command => command.TrackingPageId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tracking page ID is required.");

        RuleFor(command => command.SessionId)
            .NotEmpty()
            .MaximumLength(TrackingIdentifier.MaxLength);

        RuleFor(command => command.VisitorFingerprint)
            .NotEmpty()
            .MaximumLength(TrackingIdentifier.MaxLength);

        RuleFor(command => command.UserAgent)
            .MaximumLength(VisitEventEntity.MaxUserAgentLength)
            .When(command => !string.IsNullOrWhiteSpace(command.UserAgent));

        RuleFor(command => command.ReferrerUrl)
            .MaximumLength(VisitEventEntity.MaxReferrerLength)
            .When(command => !string.IsNullOrWhiteSpace(command.ReferrerUrl));

        RuleFor(command => command.IpHash)
            .MaximumLength(VisitEventEntity.MaxIpHashLength)
            .When(command => !string.IsNullOrWhiteSpace(command.IpHash));

        RuleFor(command => command.DeduplicationWindowSeconds)
            .InclusiveBetween(1, 3600)
            .WithMessage("Deduplication window must be between 1 and 3600 seconds.");
    }
}
