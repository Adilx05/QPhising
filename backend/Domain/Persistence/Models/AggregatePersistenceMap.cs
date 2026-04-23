using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using QPhising.Domain.Persistence.Enums;

namespace QPhising.Domain.Persistence.Models;

public sealed class AggregatePersistenceMap
{
    private readonly List<AggregatePersistenceBoundary> _boundaries = [];

    public IReadOnlyCollection<AggregatePersistenceBoundary> Boundaries =>
        new ReadOnlyCollection<AggregatePersistenceBoundary>(_boundaries);

    public static AggregatePersistenceMap CreateDefault()
    {
        return new AggregatePersistenceMap();
    }

    public void AddBoundary(AggregatePersistenceBoundary boundary)
    {
        ArgumentNullException.ThrowIfNull(boundary);

        if (_boundaries.Any(existing => string.Equals(existing.AggregateName, boundary.AggregateName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Aggregate '{boundary.AggregateName}' already has a persistence boundary definition.");
        }

        _boundaries.Add(boundary);
    }
}
