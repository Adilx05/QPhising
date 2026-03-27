using FluentValidation;

namespace QPhising.Application.Features.Templates.UpdateTemplate;

public sealed class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(command => command.TemplateId)
            .NotEmpty();

        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(command => command.HtmlContent)
            .NotEmpty();

        RuleFor(command => command.Type)
            .IsInEnum();

        RuleForEach(command => command.Variables)
            .NotEmpty();
    }
}
