using System.ComponentModel.DataAnnotations;

namespace QPhising.Infrastructure.Security;

public sealed class KeycloakValidationOptions
{
    public const string SectionName = "Keycloak";

    [Required]
    public string Authority { get; init; } = string.Empty;
}
