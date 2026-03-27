using System.ComponentModel.DataAnnotations;

namespace QPhising.Worker.Configuration;

public sealed class TaskWorkerOptions
{
    public const string SectionName = "TaskWorker";

    [Range(1, 600)]
    public int PollIntervalSeconds { get; init; } = 5;

    [Range(1, 600)]
    public int ClaimLeaseDurationSeconds { get; init; } = 60;
}
