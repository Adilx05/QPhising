using FluentValidation;

namespace QPhising.Application.Features.Tracking.ProcessTrackingClick;

public sealed class ProcessTrackingClickCommandValidator : AbstractValidator<ProcessTrackingClickCommand>
{
    public ProcessTrackingClickCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEmpty();

        RuleFor(command => command.TrackingToken)
            .NotEmpty()
            .MaximumLength(128);
    }
}
