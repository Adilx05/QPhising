namespace QPhising.Domain.Exports.Exceptions;

public sealed class ExportValidationException : ExportDomainException
{
    public ExportValidationException(string message)
        : base(message)
    {
    }
}
