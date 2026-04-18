using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed class ArchiveTemplateCommandValidator : AbstractValidator<ArchiveTemplateCommand>
{
    public ArchiveTemplateCommandValidator()
    {
        RuleFor(command => command.TemplateId)
            .NotEqual(Guid.Empty)
            .WithMessage("Template ID is required.");
    }
}
