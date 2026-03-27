using System.ComponentModel.DataAnnotations;

namespace Gateway.Configuration;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required(AllowEmptyStrings = false)]
    public string ConnectionString { get; init; } = string.Empty;
}
