namespace QPhising.Application.Exceptions;

public sealed class ApplicationAuthorizationException : Exception
{
    public ApplicationAuthorizationException(string message)
        : base(message)
    {
    }
}
