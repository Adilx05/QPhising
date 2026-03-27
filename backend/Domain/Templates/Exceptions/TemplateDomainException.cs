namespace QPhising.Domain.Templates.Exceptions;

public abstract class TemplateDomainException : Exception
{
    protected TemplateDomainException(string message)
        : base(message)
    {
    }
}
