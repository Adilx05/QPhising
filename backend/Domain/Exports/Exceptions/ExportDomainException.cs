namespace QPhising.Domain.Exports.Exceptions;

public class ExportDomainException : Exception
{
    public ExportDomainException(string message)
        : base(message)
    {
    }
}
