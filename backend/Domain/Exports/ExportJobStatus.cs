namespace QPhising.Domain.Exports;

public enum ExportJobStatus
{
    Requested = 1,
    Queued = 2,
    Processing = 3,
    Completed = 4,
    Failed = 5,
    Canceled = 6
}
