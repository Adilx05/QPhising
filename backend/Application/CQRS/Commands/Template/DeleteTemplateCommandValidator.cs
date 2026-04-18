using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed class DeleteTemplateCommandValidator : AbstractValidator<DeleteTemplateCommand>
{
    public DeleteTemplateCommandValidator()
    {
        RuleFor(command => command.TemplateId)
            .NotEqual(Guid.Empty)
            .WithMessage("Template ID is required.");
    }
}
