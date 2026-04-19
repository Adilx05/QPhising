using System;

namespace QPhising.Domain.Common;

public abstract class AuditableSoftDeletableEntity<TId> : Entity<TId>
    where TId : notnull
{
    protected AuditableSoftDeletableEntity(
        TId id,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc,
        bool isDeleted,
        DateTimeOffset? deletedAtUtc,
        string? deletedBy)
        : base(id)
    {
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc ?? createdAtUtc;
        IsDeleted = isDeleted;
        DeletedAtUtc = deletedAtUtc;
        DeletedBy = NormalizeDeletedBy(deletedBy);
    }

    public DateTimeOffset CreatedAtUtc { get; protected set; }

    public DateTimeOffset UpdatedAtUtc { get; protected set; }

    public bool IsDeleted { get; protected set; }

    public DateTimeOffset? DeletedAtUtc { get; protected set; }

    public string? DeletedBy { get; protected set; }

    protected void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;

    public virtual void MarkDeleted(string? deletedBy = null)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        DeletedBy = NormalizeDeletedBy(deletedBy);
        Touch();
    }

    private static string? NormalizeDeletedBy(string? deletedBy)
    {
        if (string.IsNullOrWhiteSpace(deletedBy))
        {
            return null;
        }

        return deletedBy.Trim();
    }
}
