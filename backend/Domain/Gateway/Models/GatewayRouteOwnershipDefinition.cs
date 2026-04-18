using System;

using QPhising.Domain.Gateway.Enums;
using QPhising.Domain.Gateway.ValueObjects;

namespace QPhising.Domain.Gateway.Models;

public sealed class GatewayRouteOwnershipDefinition
{
    public GatewayRouteOwnershipDefinition(
        GatewayRouteTemplate upstreamPathTemplate,
        GatewayModule owner,
        bool requiresAuthentication,
        string purpose)
    {
        ArgumentNullException.ThrowIfNull(upstreamPathTemplate);

        if (string.IsNullOrWhiteSpace(purpose))
        {
            throw new ArgumentException("Gateway route purpose is required.", nameof(purpose));
        }

        UpstreamPathTemplate = upstreamPathTemplate;
        Owner = owner;
        RequiresAuthentication = requiresAuthentication;
        Purpose = purpose.Trim();
    }

    public GatewayRouteTemplate UpstreamPathTemplate { get; }

    public GatewayModule Owner { get; }

    public bool RequiresAuthentication { get; }

    public string Purpose { get; }
}
