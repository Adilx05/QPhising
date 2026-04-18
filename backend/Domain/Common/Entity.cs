using System;
using System.Collections.Generic;

namespace QPhising.Domain.Common;

public abstract class Entity<TId>
    where TId : notnull
{
    private readonly List<DomainEvent> _domainEvents = [];

    protected Entity(TId id)
    {
        if (EqualityComparer<TId>.Default.Equals(id, default!))
        {
            throw new ArgumentException("Entity ID cannot be empty.", nameof(id));
        }

        Id = id;
    }

    public TId Id { get; }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(DomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
