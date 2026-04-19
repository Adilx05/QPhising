using FluentValidation;
using QPhising.Domain.Templates.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Template name is required.")
            .MaximumLength(TemplateName.MaxLength)
            .WithMessage($"Template name must be at most {TemplateName.MaxLength} characters.");

        RuleFor(command => command.HtmlContent)
            .NotEmpty()
            .WithMessage("Template HTML content is required.")
            .MaximumLength(TemplateContent.MaxHtmlLength)
            .WithMessage($"Template HTML content must be at most {TemplateContent.MaxHtmlLength} characters.");

        RuleFor(command => command.Description)
            .MaximumLength(TemplateMetadata.MaxDescriptionLength)
            .WithMessage($"Template description must be at most {TemplateMetadata.MaxDescriptionLength} characters.")
            .When(command => !string.IsNullOrWhiteSpace(command.Description));

        RuleFor(command => command.Tags)
            .Must(HaveValidTagCount)
            .WithMessage($"Template metadata supports at most {TemplateMetadata.MaxTagCount} tags.");

        RuleForEach(command => command.Tags)
            .NotEmpty()
            .WithMessage("Template tags cannot be empty.")
            .MaximumLength(TemplateMetadata.MaxTagLength)
            .WithMessage($"Each template tag must be at most {TemplateMetadata.MaxTagLength} characters.");
    }

    private static bool HaveValidTagCount(IReadOnlyCollection<string>? tags)
    {
        if (tags is null)
        {
            return true;
        }

        return tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() <= TemplateMetadata.MaxTagCount;
    }
}
