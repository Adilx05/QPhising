using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class DeleteTrackingPageCommandValidator : AbstractValidator<DeleteTrackingPageCommand>
{
    public DeleteTrackingPageCommandValidator()
    {
        RuleFor(command => command.TrackingPageId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tracking page ID is required.");
    }
}
