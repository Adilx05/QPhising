using System.Collections.ObjectModel;

namespace QPhising.Application.Contracts.Responses.Gateway;

public sealed record GatewayRoutePolicySettings(
    string AuthenticationProviderKey,
    IReadOnlyDictionary<string, string> ClaimsToHeaders,
    bool ForwardAccessToken)
{
    public static GatewayRoutePolicySettings Default() =>
        new(
            AuthenticationProviderKey: "Bearer",
            ClaimsToHeaders: new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["sub"] = "X-QP-UserId",
                ["preferred_username"] = "X-QP-Username",
                ["realm_access.roles"] = "X-QP-Roles"
            }),
            ForwardAccessToken: true);
}
