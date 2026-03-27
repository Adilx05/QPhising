namespace QPhising.Domain.Templates.Exceptions;

public sealed class InvalidTemplateStatusTransitionException : TemplateDomainException
{
    public InvalidTemplateStatusTransitionException(TemplateStatus currentStatus, TemplateStatus requestedStatus)
        : base($"Template status transition from '{currentStatus}' to '{requestedStatus}' is not allowed.")
    {
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }

    public TemplateStatus CurrentStatus { get; }

    public TemplateStatus RequestedStatus { get; }
}
