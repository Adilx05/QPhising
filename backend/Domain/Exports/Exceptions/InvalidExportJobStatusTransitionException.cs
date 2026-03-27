using QPhising.Domain.Exports;

namespace QPhising.Domain.Exports.Exceptions;

public sealed class InvalidExportJobStatusTransitionException : ExportDomainException
{
    public InvalidExportJobStatusTransitionException(ExportJobStatus currentStatus, ExportJobStatus requestedStatus)
        : base($"Export job status transition from '{currentStatus}' to '{requestedStatus}' is not allowed.")
    {
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }

    public ExportJobStatus CurrentStatus { get; }

    public ExportJobStatus RequestedStatus { get; }
}
