using System;

namespace QPhising.Domain.Common;

public abstract record DomainEvent
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }
}
