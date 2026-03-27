using System.ComponentModel.DataAnnotations;

namespace QPhising.API.Configuration;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    [Required(AllowEmptyStrings = false)]
    public string Host { get; init; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; init; } = 587;

    [Required(AllowEmptyStrings = false)]
    public string Username { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Password { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    public string FromAddress { get; init; } = string.Empty;

    public bool UseSsl { get; init; } = true;
}
