namespace QPhising.Domain.Tasks.Exceptions;

public class TaskDomainException : Exception
{
    public TaskDomainException(string message)
        : base(message)
    {
    }
}
