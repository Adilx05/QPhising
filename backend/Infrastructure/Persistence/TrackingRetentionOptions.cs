using System.ComponentModel.DataAnnotations;

namespace QPhising.Infrastructure.Persistence;

public sealed class TrackingRetentionOptions
{
    public const string SectionName = "TrackingRetention";

    [Range(1, 3650)]
    public int RawClickRetentionDays { get; init; } = 90;

    [Range(1, 3650)]
    public int AggregateRetentionDays { get; init; } = 365;

    [Range(1, 1440)]
    public int CleanupIntervalMinutes { get; init; } = 60;

    [Range(1, 10000)]
    public int CleanupBatchSize { get; init; } = 1000;
}
