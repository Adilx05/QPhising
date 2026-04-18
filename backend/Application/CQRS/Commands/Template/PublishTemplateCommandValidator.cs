using FluentValidation;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed class PublishTemplateCommandValidator : AbstractValidator<PublishTemplateCommand>
{
    public PublishTemplateCommandValidator()
    {
        RuleFor(command => command.TemplateId)
            .NotEqual(Guid.Empty)
            .WithMessage("Template ID is required.");
    }
}
