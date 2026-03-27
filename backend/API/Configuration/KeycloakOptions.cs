using System.ComponentModel.DataAnnotations;

namespace QPhising.API.Configuration;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";

    [Required(AllowEmptyStrings = false)]
    public string Authority { get; init; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Audience { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; } = true;
}
