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
            .MaximumLength(512);

        RuleFor(command => command.IpAddress)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(command => command.UserAgent)
            .NotEmpty()
            .MaximumLength(1024);

        RuleFor(command => command.Fingerprint)
            .MaximumLength(256);
    }
}
