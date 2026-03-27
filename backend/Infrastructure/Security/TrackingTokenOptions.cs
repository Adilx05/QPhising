using System.ComponentModel.DataAnnotations;

namespace QPhising.Infrastructure.Security;

public sealed class TrackingTokenOptions
{
    public const string SectionName = "TrackingTokens";

    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpirationMinutes { get; set; } = 30;

    [Range(1, 4)]
    public int Version { get; set; } = 1;
}
