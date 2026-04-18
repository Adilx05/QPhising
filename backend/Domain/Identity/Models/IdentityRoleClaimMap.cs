using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using QPhising.Domain.Identity.Enums;
using QPhising.Domain.Identity.ValueObjects;

namespace QPhising.Domain.Identity.Models;

public sealed class IdentityRoleClaimMap
{
    private readonly List<IdentityRoleClaimMapping> _mappings = [];

    public IReadOnlyCollection<IdentityRoleClaimMapping> Mappings => new ReadOnlyCollection<IdentityRoleClaimMapping>(_mappings);

    public static IdentityRoleClaimMap CreateDefault()
    {
        var map = new IdentityRoleClaimMap();

        map.AddMapping(new IdentityRoleClaimMapping(
            IdentityRole.Admin,
            [
                new IdentityClaim("role", "Admin"),
                new IdentityClaim("roles", "Admin"),
                new IdentityClaim("realm_access.roles", "Admin"),
                new IdentityClaim("resource_access.qphising-api.roles", "Admin")
            ]));

        map.AddMapping(new IdentityRoleClaimMapping(
            IdentityRole.Operator,
            [
                new IdentityClaim("role", "Operator"),
                new IdentityClaim("roles", "Operator"),
                new IdentityClaim("realm_access.roles", "Operator"),
                new IdentityClaim("resource_access.qphising-api.roles", "Operator")
            ]));

        map.AddMapping(new IdentityRoleClaimMapping(
            IdentityRole.Viewer,
            [
                new IdentityClaim("role", "Viewer"),
                new IdentityClaim("roles", "Viewer"),
                new IdentityClaim("realm_access.roles", "Viewer"),
                new IdentityClaim("resource_access.qphising-api.roles", "Viewer")
            ]));

        return map;
    }

    public void AddMapping(IdentityRoleClaimMapping mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        if (_mappings.Any(existing => existing.Role == mapping.Role))
        {
            throw new InvalidOperationException($"Identity role mapping for '{mapping.Role}' already exists.");
        }

        _mappings.Add(mapping);
    }

    public IdentityRole? ResolveRole(IEnumerable<IdentityClaim> claims)
    {
        ArgumentNullException.ThrowIfNull(claims);

        var claimSet = claims
            .Distinct()
            .ToHashSet();

        if (claimSet.Count == 0)
        {
            return null;
        }

        return _mappings
            .FirstOrDefault(mapping => mapping.AcceptedClaims.Any(claimSet.Contains))
            ?.Role;
    }
}
