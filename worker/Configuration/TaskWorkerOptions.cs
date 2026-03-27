using System.ComponentModel.DataAnnotations;

namespace QPhising.Worker.Configuration;

public sealed class TaskWorkerOptions
{
    public const string SectionName = "TaskWorker";

    [Range(1, 600)]
    public int PollIntervalSeconds { get; init; } = 5;

    [Range(1, 600)]
    public int ClaimLeaseDurationSeconds { get; init; } = 60;

    [Range(1, 3600)]
    public int InitialRetryDelaySeconds { get; init; } = 5;

    [Range(1, 86400)]
    public int MaxRetryDelaySeconds { get; init; } = 300;

    [Range(1, 5)]
    public int RetryBackoffMultiplier { get; init; } = 2;
}
