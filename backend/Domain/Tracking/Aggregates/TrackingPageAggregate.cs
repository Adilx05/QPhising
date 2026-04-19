using QPhising.Domain.Common;
using QPhising.Domain.Tracking.Enums;
using QPhising.Domain.Tracking.Models;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Domain.Tracking.Aggregates;

public sealed class TrackingPageAggregate : Entity<Guid>
{
    public const int MaxTitleLength = 160;
    public const int MaxDescriptionLength = 1000;
    public const int MaxOwnerIdLength = 128;

    public TrackingPageAggregate(
        Guid id,
        TrackingPageSlug slug,
        string title,
        string? description,
        TrackingPageUrl destinationUrl,
        string ownerId,
        Guid? templateId,
        PageSettings? settings = null)
        : this(
            id,
            slug,
            title,
            description,
            destinationUrl,
            ownerId,
            templateId,
            TrackingPagePublishState.Draft,
            settings,
            DateTimeOffset.UtcNow,
            null)
    {
    }

    private TrackingPageAggregate(
        Guid id,
        TrackingPageSlug slug,
        string title,
        string? description,
        TrackingPageUrl destinationUrl,
        string ownerId,
        Guid? templateId,
        TrackingPagePublishState publishState,
        PageSettings? settings,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(destinationUrl);

        Slug = slug;
        Title = NormalizeTitle(title);
        Description = NormalizeDescription(description);
        DestinationUrl = destinationUrl;
        OwnerId = NormalizeOwnerId(ownerId);
        TemplateId = NormalizeTemplateId(templateId);
        PublishState = publishState;
        Settings = settings;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc ?? createdAtUtc;
    }

    public TrackingPageSlug Slug { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public TrackingPageUrl DestinationUrl { get; private set; }

    public string OwnerId { get; private set; }

    public Guid? TemplateId { get; private set; }

    public TrackingPagePublishState PublishState { get; private set; }

    public PageSettings? Settings { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public void UpdateDetails(TrackingPageSlug slug, string title, string? description, TrackingPageUrl destinationUrl, Guid? templateId)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(destinationUrl);

        Slug = slug;
        Title = NormalizeTitle(title);
        Description = NormalizeDescription(description);
        DestinationUrl = destinationUrl;
        TemplateId = NormalizeTemplateId(templateId);
        Touch();
    }

    public void AssignOwner(string ownerId)
    {
        EnsureMutable();
        OwnerId = NormalizeOwnerId(ownerId);
        Touch();
    }

    public void ConfigureSettings(PageSettings? settings)
    {
        EnsureMutable();
        Settings = settings;
        Touch();
    }

    public void Publish()
    {
        if (PublishState == TrackingPagePublishState.Archived)
        {
            throw new InvalidOperationException("Archived tracking pages cannot be published.");
        }

        PublishState = TrackingPagePublishState.Published;
        Touch();
    }

    public void Archive()
    {
        PublishState = TrackingPagePublishState.Archived;
        Touch();
    }

    public static TrackingPageAggregate Rehydrate(
        Guid id,
        TrackingPageSlug slug,
        string title,
        string? description,
        TrackingPageUrl destinationUrl,
        string ownerId,
        Guid? templateId,
        TrackingPagePublishState publishState,
        PageSettings? settings,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        return new TrackingPageAggregate(id, slug, title, description, destinationUrl, ownerId, templateId, publishState, settings, createdAtUtc, updatedAtUtc);
    }

    private void EnsureMutable()
    {
        if (PublishState == TrackingPagePublishState.Archived)
        {
            throw new InvalidOperationException("Archived tracking pages cannot be modified.");
        }
    }

    private static string NormalizeTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tracking page title is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxTitleLength)
        {
            throw new ArgumentException($"Tracking page title cannot exceed {MaxTitleLength} characters.", nameof(value));
        }

        return normalized;
    }

    private static string? NormalizeDescription(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxDescriptionLength)
        {
            throw new ArgumentException($"Tracking page description cannot exceed {MaxDescriptionLength} characters.", nameof(value));
        }

        return normalized;
    }

    private static string NormalizeOwnerId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tracking page owner is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (normalized.Length > MaxOwnerIdLength)
        {
            throw new ArgumentException($"Tracking page owner cannot exceed {MaxOwnerIdLength} characters.", nameof(value));
        }

        return normalized;
    }

    private static Guid? NormalizeTemplateId(Guid? templateId)
    {
        if (!templateId.HasValue)
        {
            return null;
        }

        return templateId.Value == Guid.Empty ? null : templateId.Value;
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
