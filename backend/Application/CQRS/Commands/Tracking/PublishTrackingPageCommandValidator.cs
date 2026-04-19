using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class PublishTrackingPageCommandValidator : AbstractValidator<PublishTrackingPageCommand>
{
    public PublishTrackingPageCommandValidator()
    {
        RuleFor(command => command.TrackingPageId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tracking page ID is required.");
    }
}
