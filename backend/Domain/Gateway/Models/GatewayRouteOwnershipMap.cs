using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using QPhising.Domain.Gateway.Enums;
using QPhising.Domain.Gateway.ValueObjects;

namespace QPhising.Domain.Gateway.Models;

public sealed class GatewayRouteOwnershipMap
{
    private readonly List<GatewayRouteOwnershipDefinition> _definitions = [];

    public IReadOnlyCollection<GatewayRouteOwnershipDefinition> Definitions =>
        new ReadOnlyCollection<GatewayRouteOwnershipDefinition>(_definitions);

    public static GatewayRouteOwnershipMap CreateDefault()
    {
        var map = new GatewayRouteOwnershipMap();

        map.AddDefinition(new GatewayRouteOwnershipDefinition(
            new GatewayRouteTemplate("/api/{everything}"),
            GatewayModule.PlatformApi,
            requiresAuthentication: true,
            purpose: "Authenticated API surface for runtime application modules."));

        map.AddDefinition(new GatewayRouteOwnershipDefinition(
            new GatewayRouteTemplate("/health/live"),
            GatewayModule.Health,
            requiresAuthentication: false,
            purpose: "Gateway liveness path for health probing."));

        return map;
    }

    public void AddDefinition(GatewayRouteOwnershipDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (_definitions.Any(existing => existing.UpstreamPathTemplate.Equals(definition.UpstreamPathTemplate)))
        {
            throw new InvalidOperationException($"Gateway route '{definition.UpstreamPathTemplate}' is already mapped.");
        }

        _definitions.Add(definition);
    }
}
