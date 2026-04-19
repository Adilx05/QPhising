using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Tracking;

public sealed class ArchiveTrackingPageCommandValidator : AbstractValidator<ArchiveTrackingPageCommand>
{
    public ArchiveTrackingPageCommandValidator()
    {
        RuleFor(command => command.TrackingPageId)
            .NotEqual(Guid.Empty)
            .WithMessage("Tracking page ID is required.");
    }
}
