using System.ComponentModel.DataAnnotations;

namespace QPhising.Infrastructure.Persistence;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; init; } = string.Empty;

    [Range(1, 1440)]
    public int TrackingDeduplicationWindowMinutes { get; init; } = 15;

    [Required(AllowEmptyStrings = false)]
    public string KeyPrefix { get; init; } = "qphising";
}
