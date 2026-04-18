using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using QPhising.Domain.Identity.Enums;
using QPhising.Domain.Identity.ValueObjects;

namespace QPhising.Domain.Identity.Models;

public sealed class IdentityRoleClaimMapping
{
    private readonly List<IdentityClaim> _acceptedClaims = [];

    public IdentityRoleClaimMapping(IdentityRole role, IEnumerable<IdentityClaim> acceptedClaims)
    {
        ArgumentNullException.ThrowIfNull(acceptedClaims);

        var normalizedClaims = acceptedClaims
            .Distinct()
            .ToList();

        if (normalizedClaims.Count == 0)
        {
            throw new ArgumentException("At least one accepted claim must be defined for a role.", nameof(acceptedClaims));
        }

        Role = role;
        _acceptedClaims.AddRange(normalizedClaims);
    }

    public IdentityRole Role { get; }

    public IReadOnlyCollection<IdentityClaim> AcceptedClaims => new ReadOnlyCollection<IdentityClaim>(_acceptedClaims);

    public bool Matches(IdentityClaim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);

        return _acceptedClaims.Contains(claim);
    }
}
