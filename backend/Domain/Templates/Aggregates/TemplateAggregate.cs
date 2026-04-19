using System;

using QPhising.Domain.Common;
using QPhising.Domain.Templates.Enums;
using QPhising.Domain.Templates.ValueObjects;

namespace QPhising.Domain.Templates.Aggregates;

public sealed class TemplateAggregate : AuditableSoftDeletableEntity<Guid>
{
    public const int InitialVersion = 1;
    public const int MaxVersion = 10_000;

    public TemplateAggregate(Guid id, TemplateName name, TemplateContent content, TemplateMetadata metadata)
        : this(
            id,
            name,
            content,
            metadata,
            TemplateLifecycleState.Draft,
            InitialVersion,
            createdAtUtc: DateTimeOffset.UtcNow,
            updatedAtUtc: null,
            isDeleted: false,
            deletedAtUtc: null,
            deletedBy: null)
    {
    }

    private TemplateAggregate(
        Guid id,
        TemplateName name,
        TemplateContent content,
        TemplateMetadata metadata,
        TemplateLifecycleState lifecycleState,
        int version,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc,
        bool isDeleted,
        DateTimeOffset? deletedAtUtc,
        string? deletedBy)
        : base(id, createdAtUtc, updatedAtUtc, isDeleted, deletedAtUtc, deletedBy)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(metadata);

        ValidateVersion(version);

        Name = name;
        Content = content;
        Metadata = metadata;
        LifecycleState = lifecycleState;
        Version = version;
    }

    public TemplateName Name { get; private set; }

    public TemplateContent Content { get; private set; }

    public TemplateMetadata Metadata { get; private set; }

    public TemplateLifecycleState LifecycleState { get; private set; }

    public int Version { get; private set; }

    public void Update(TemplateName name, TemplateContent content, TemplateMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(metadata);

        EnsureMutable();

        var hasChanges = Name != name || Content != content || Metadata != metadata;
        if (!hasChanges)
        {
            return;
        }

        Name = name;
        Content = content;
        Metadata = metadata;
        IncrementVersion();
        Touch();
    }

    public void Publish()
    {
        if (LifecycleState == TemplateLifecycleState.Archived)
        {
            throw new InvalidOperationException("Archived templates cannot be published.");
        }

        LifecycleState = TemplateLifecycleState.Published;
        Touch();
    }

    public void Archive()
    {
        LifecycleState = TemplateLifecycleState.Archived;
        Touch();
    }

    public static TemplateAggregate Rehydrate(
        Guid id,
        TemplateName name,
        TemplateContent content,
        TemplateMetadata metadata,
        TemplateLifecycleState lifecycleState,
        int version,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        bool isDeleted = false,
        DateTimeOffset? deletedAtUtc = null,
        string? deletedBy = null)
    {
        return new TemplateAggregate(
            id,
            name,
            content,
            metadata,
            lifecycleState,
            version,
            createdAtUtc,
            updatedAtUtc,
            isDeleted,
            deletedAtUtc,
            deletedBy);
    }

    private void EnsureMutable()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Deleted templates cannot be modified.");
        }

        if (LifecycleState == TemplateLifecycleState.Archived)
        {
            throw new InvalidOperationException("Archived templates cannot be modified.");
        }
    }

    private void IncrementVersion()
    {
        if (Version >= MaxVersion)
        {
            throw new InvalidOperationException($"Template version exceeded the max allowed value of {MaxVersion}.");
        }

        Version++;
    }

    private static void ValidateVersion(int version)
    {
        if (version < InitialVersion)
        {
            throw new ArgumentOutOfRangeException(nameof(version), $"Template version must be at least {InitialVersion}.");
        }

        if (version > MaxVersion)
        {
            throw new ArgumentOutOfRangeException(nameof(version), $"Template version cannot exceed {MaxVersion}.");
        }
    }

}
