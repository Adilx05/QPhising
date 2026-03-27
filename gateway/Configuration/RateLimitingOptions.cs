using System.ComponentModel.DataAnnotations;

namespace Gateway.Configuration;

public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    [Range(1, int.MaxValue)]
    public int DefaultWindowSeconds { get; init; } = 60;

    [Range(1, int.MaxValue)]
    public int DefaultLimit { get; init; } = 120;

    [Required]
    public IReadOnlyList<RateLimitRuleOptions> Rules { get; init; } = [];
}

public sealed class RateLimitRuleOptions
{
    [Required]
    public string PathPrefix { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Limit { get; init; }

    [Range(1, int.MaxValue)]
    public int WindowSeconds { get; init; } = 60;

    [Required]
    public IReadOnlyList<string> Methods { get; init; } = ["GET", "POST", "PUT", "DELETE", "PATCH"];
}
