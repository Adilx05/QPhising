using FluentValidation;

namespace QPhising.Application.Features.Templates.CreateTemplate;

public sealed class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
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
