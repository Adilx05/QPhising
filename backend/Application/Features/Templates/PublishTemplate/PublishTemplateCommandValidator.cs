using FluentValidation;

namespace QPhising.Application.Features.Templates.PublishTemplate;

public sealed class PublishTemplateCommandValidator : AbstractValidator<PublishTemplateCommand>
{
    public PublishTemplateCommandValidator()
    {
        RuleFor(command => command.TemplateId)
            .NotEmpty();
    }
}
