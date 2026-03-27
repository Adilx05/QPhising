using FluentValidation;

namespace QPhising.Application.Features.Exports.QueueExportJob;

public sealed class QueueExportJobCommandValidator : AbstractValidator<QueueExportJobCommand>
{
    public QueueExportJobCommandValidator()
    {
        RuleFor(command => command.OwnerUserId)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.CorrelationId)
            .MaximumLength(100)
            .When(command => !string.IsNullOrWhiteSpace(command.CorrelationId));
    }
}
