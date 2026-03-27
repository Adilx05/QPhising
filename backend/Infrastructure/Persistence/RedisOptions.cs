using System.ComponentModel.DataAnnotations;

namespace QPhising.Infrastructure.Persistence;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; init; } = string.Empty;
}
