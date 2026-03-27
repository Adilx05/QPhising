namespace QPhising.Domain.Tasks.Exceptions;

public sealed class TaskValidationException : TaskDomainException
{
    public TaskValidationException(string message)
        : base(message)
    {
    }
}
