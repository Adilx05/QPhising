using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using QPhising.Domain.Persistence.Enums;

namespace QPhising.Domain.Persistence.Models;

public sealed class AggregatePersistenceBoundary
{
    private readonly List<AggregateFieldMapping> _fields = [];

    public AggregatePersistenceBoundary(
        string aggregateName,
        AggregateStorageKind storageKind,
        string storageObject,
        string ownershipBoundary,
        IEnumerable<AggregateFieldMapping> fields)
    {
        AggregateName = string.IsNullOrWhiteSpace(aggregateName)
            ? throw new ArgumentException("Aggregate name is required.", nameof(aggregateName))
            : aggregateName.Trim();

        StorageObject = string.IsNullOrWhiteSpace(storageObject)
            ? throw new ArgumentException("Storage object is required.", nameof(storageObject))
            : storageObject.Trim();

        OwnershipBoundary = string.IsNullOrWhiteSpace(ownershipBoundary)
            ? throw new ArgumentException("Ownership boundary is required.", nameof(ownershipBoundary))
            : ownershipBoundary.Trim();

        StorageKind = storageKind;

        ArgumentNullException.ThrowIfNull(fields);

        var normalized = fields.ToList();

        if (normalized.Count == 0)
        {
            throw new ArgumentException("At least one mapped field is required.", nameof(fields));
        }

        _fields.AddRange(normalized);
    }

    public string AggregateName { get; }

    public AggregateStorageKind StorageKind { get; }

    public string StorageObject { get; }

    public string OwnershipBoundary { get; }

    public IReadOnlyCollection<AggregateFieldMapping> Fields => new ReadOnlyCollection<AggregateFieldMapping>(_fields);
}
