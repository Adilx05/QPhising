using FluentValidation;

namespace QPhising.Application.Features.Templates.ArchiveTemplate;

public sealed class ArchiveTemplateCommandValidator : AbstractValidator<ArchiveTemplateCommand>
{
    public ArchiveTemplateCommandValidator()
    {
        RuleFor(command => command.TemplateId)
            .NotEmpty();
    }
}
