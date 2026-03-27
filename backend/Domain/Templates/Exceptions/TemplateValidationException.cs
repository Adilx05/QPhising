namespace QPhising.Domain.Templates.Exceptions;

public sealed class TemplateValidationException : TemplateDomainException
{
    public TemplateValidationException(string message)
        : base(message)
    {
    }
}
