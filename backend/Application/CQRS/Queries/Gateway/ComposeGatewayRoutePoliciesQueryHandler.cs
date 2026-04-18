using MediatR;
using QPhising.Application.Contracts.Abstractions.Gateway;
using QPhising.Application.Contracts.Responses.Gateway;
using QPhising.Domain.Gateway.Models;

namespace QPhising.Application.CQRS.Queries.Gateway;

public sealed class ComposeGatewayRoutePoliciesQueryHandler : IRequestHandler<ComposeGatewayRoutePoliciesQuery, GatewayRoutePolicyCompositionResult>
{
    private readonly IGatewayRoutePolicySettingsProvider _settingsProvider;

    public ComposeGatewayRoutePoliciesQueryHandler(IGatewayRoutePolicySettingsProvider settingsProvider)
    {
        _settingsProvider = settingsProvider;
    }

    public async Task<GatewayRoutePolicyCompositionResult> Handle(ComposeGatewayRoutePoliciesQuery request, CancellationToken cancellationToken)
    {
        var settings = await _settingsProvider.GetCurrentAsync(cancellationToken);
        var ownershipMap = GatewayRouteOwnershipMap.CreateDefault();

        var routePolicies = ownershipMap.Definitions
            .Select(definition => new GatewayRoutePolicyDefinitionResult(
                UpstreamPathTemplate: definition.UpstreamPathTemplate.Value,
                Owner: definition.Owner,
                RequiresAuthentication: definition.RequiresAuthentication,
                AuthenticationProviderKey: definition.RequiresAuthentication ? settings.AuthenticationProviderKey : null,
                ForwardAccessToken: definition.RequiresAuthentication && settings.ForwardAccessToken,
                ClaimsToHeaders: definition.RequiresAuthentication
                    ? settings.ClaimsToHeaders
                    : new Dictionary<string, string>(),
                Purpose: definition.Purpose))
            .ToArray();

        return new GatewayRoutePolicyCompositionResult(routePolicies);
    }
}
