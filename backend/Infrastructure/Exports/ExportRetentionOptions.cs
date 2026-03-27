using System.ComponentModel.DataAnnotations;

namespace QPhising.Infrastructure.Exports;

public sealed class ExportRetentionOptions
{
    public const string SectionName = "ExportRetention";

    [Range(1, 1440)]
    public int CleanupIntervalMinutes { get; set; } = 60;

    [Range(1, 5000)]
    public int CleanupBatchSize { get; set; } = 250;
}
