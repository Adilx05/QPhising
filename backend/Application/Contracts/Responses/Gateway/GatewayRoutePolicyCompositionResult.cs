namespace QPhising.Application.Contracts.Responses.Gateway;

public sealed record GatewayRoutePolicyCompositionResult(
    IReadOnlyCollection<GatewayRoutePolicyDefinitionResult> Routes);
