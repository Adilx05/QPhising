using QPhising.Domain.Gateway.Enums;

namespace QPhising.Application.Contracts.Responses.Gateway;

public sealed record GatewayRoutePolicyDefinitionResult(
    string UpstreamPathTemplate,
    GatewayModule Owner,
    bool RequiresAuthentication,
    string? AuthenticationProviderKey,
    bool ForwardAccessToken,
    IReadOnlyDictionary<string, string> ClaimsToHeaders,
    string Purpose);
