using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Campaign;

public sealed class ScheduleCampaignCommandValidator : AbstractValidator<ScheduleCampaignCommand>
{
    public ScheduleCampaignCommandValidator()
    {
        RuleFor(command => command.CampaignId)
            .NotEqual(Guid.Empty)
            .WithMessage("Campaign ID is required.");

        RuleFor(command => command.StartsAtUtc)
            .NotEqual(DateTimeOffset.MinValue)
            .WithMessage("Campaign start time is required.");

        RuleFor(command => command)
            .Must(command => !command.EndsAtUtc.HasValue || command.EndsAtUtc.Value > command.StartsAtUtc)
            .WithMessage("Campaign end time must be after the start time.");
    }
}
