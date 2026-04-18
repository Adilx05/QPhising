using FluentValidation;
using QPhising.Domain.Templates.ValueObjects;

namespace QPhising.Application.CQRS.Commands.Template;

public sealed class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(command => command.TemplateId)
            .NotEqual(Guid.Empty)
            .WithMessage("Template ID is required.");

        RuleFor(command => command.Name)
            .NotEmpty()
            .WithMessage("Template name is required.")
            .MaximumLength(TemplateName.MaxLength)
            .WithMessage($"Template name must be at most {TemplateName.MaxLength} characters.");

        RuleFor(command => command.Subject)
            .NotEmpty()
            .WithMessage("Template subject is required.")
            .MaximumLength(TemplateContent.MaxSubjectLength)
            .WithMessage($"Template subject must be at most {TemplateContent.MaxSubjectLength} characters.");

        RuleFor(command => command.Body)
            .NotEmpty()
            .WithMessage("Template body is required.")
            .MaximumLength(TemplateContent.MaxBodyLength)
            .WithMessage($"Template body must be at most {TemplateContent.MaxBodyLength} characters.");

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
