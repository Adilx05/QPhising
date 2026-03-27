using System.ComponentModel.DataAnnotations;

namespace QPhising.Infrastructure.Persistence;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int TrackingDeduplicationWindowMinutes { get; init; } = 15;

    [Range(0, 300)]
    public int TrackingTokenClockSkewSeconds { get; init; } = 30;

    [Range(1, 1440)]
    public int TrackingAbuseWindowMinutes { get; init; } = 1;

    [Range(1, 100000)]
    public int TrackingSuspiciousIpThreshold { get; init; } = 20;

    [Range(1, 100000)]
    public int TrackingIpRejectionThreshold { get; init; } = 50;

    [Range(1, 3650)]
    public int TrackingAggregateRetentionDays { get; init; } = 365;

    [Range(30, 86400)]
    public int AnalyticsDashboardCacheTtlSeconds { get; init; } = 300;

    [Required(AllowEmptyStrings = false)]
    public string KeyPrefix { get; init; } = "qphising";
}
