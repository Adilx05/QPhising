using QPhising.Domain.Common;
using QPhising.Domain.Tracking.Enums;
using QPhising.Domain.Tracking.Models;
using QPhising.Domain.Tracking.ValueObjects;

namespace QPhising.Domain.Tracking.Aggregates;

public sealed class TrackingPageAggregate : AuditableSoftDeletableEntity<Guid>
{
    public const int MaxTitleLength = 160;
    public const int MaxDescriptionLength = 1000;
    public const int MaxOwnerIdLength = 128;
    public const int MaxCustomHtmlContentLength = 200000;

    public TrackingPageAggregate(
        Guid id,
        TrackingPageSlug slug,
        string title,
        string? description,
        string ownerId,
        Guid? templateId,
        string? customHtmlContent,
        DateTimeOffset? validFromUtc,
        DateTimeOffset? validUntilUtc,
        PageSettings? settings = null)
        : this(
            id,
            slug,
            title,
            description,
            ownerId,
            templateId,
            customHtmlContent,
            validFromUtc,
            validUntilUtc,
            TrackingPagePublishState.Draft,
            settings,
            DateTimeOffset.UtcNow,
            null,
            isDeleted: false,
            deletedAtUtc: null,
            deletedBy: null)
    {
    }

    private TrackingPageAggregate(
        Guid id,
        TrackingPageSlug slug,
        string title,
        string? description,
        string ownerId,
        Guid? templateId,
        string? customHtmlContent,
        DateTimeOffset? validFromUtc,
        DateTimeOffset? validUntilUtc,
        TrackingPagePublishState publishState,
        PageSettings? settings,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc,
        bool isDeleted,
        DateTimeOffset? deletedAtUtc,
        string? deletedBy)
        : base(id, createdAtUtc, updatedAtUtc, isDeleted, deletedAtUtc, deletedBy)
    {
        ArgumentNullException.ThrowIfNull(slug);

        Slug = slug;
        Title = NormalizeTitle(title);
        Description = NormalizeDescription(description);
        OwnerId = NormalizeOwnerId(ownerId);
        TemplateId = NormalizeTemplateId(templateId);
        CustomHtmlContent = NormalizeCustomHtmlContent(customHtmlContent);
        ApplyValidityWindow(validFromUtc, validUntilUtc);
        PublishState = publishState;
        Settings = settings;
    }

    public TrackingPageSlug Slug { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public string OwnerId { get; private set; }

    public Guid? TemplateId { get; private set; }

    public string? CustomHtmlContent { get; private set; }

    public DateTimeOffset? ValidFromUtc { get; private set; }

    public DateTimeOffset? ValidUntilUtc { get; private set; }

    public TrackingPagePublishState PublishState { get; private set; }

    public PageSettings? Settings { get; private set; }

    public void UpdateDetails(
        TrackingPageSlug slug,
        string title,
        string? description,
        Guid? templateId,
        string? customHtmlContent,
        DateTimeOffset? validFromUtc,
        DateTimeOffset? validUntilUtc)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(slug);

        Slug = slug;
        Title = NormalizeTitle(title);
        Description = NormalizeDescription(description);
        TemplateId = NormalizeTemplateId(templateId);
        CustomHtmlContent = NormalizeCustomHtmlContent(customHtmlContent);
        ApplyValidityWindow(validFromUtc, validUntilUtc);
        Touch();
    }

    public bool IsPubliclyAccessible(DateTimeOffset nowUtc)
    {
        if (PublishState != TrackingPagePublishState.Published)
        {
            return false;
        }

        if (ValidFromUtc.HasValue && nowUtc < ValidFromUtc.Value)
        {
            return false;
        }

        if (ValidUntilUtc.HasValue && nowUtc > ValidUntilUtc.Value)
        {
            return false;
        }

        return true;
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
        string ownerId,
        Guid? templateId,
        string? customHtmlContent,
        DateTimeOffset? validFromUtc,
        DateTimeOffset? validUntilUtc,
        TrackingPagePublishState publishState,
        PageSettings? settings,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        bool isDeleted = false,
        DateTimeOffset? deletedAtUtc = null,
        string? deletedBy = null)
    {
        return new TrackingPageAggregate(
            id,
            slug,
            title,
            description,
            ownerId,
            templateId,
            customHtmlContent,
            validFromUtc,
            validUntilUtc,
            publishState,
            settings,
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
            throw new InvalidOperationException("Deleted tracking pages cannot be modified.");
        }

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

    private static string? NormalizeCustomHtmlContent(string? customHtmlContent)
    {
        if (string.IsNullOrWhiteSpace(customHtmlContent))
        {
            return null;
        }

        var normalized = customHtmlContent.Trim();
        if (normalized.Length > MaxCustomHtmlContentLength)
        {
            throw new ArgumentException($"Tracking page custom HTML cannot exceed {MaxCustomHtmlContentLength} characters.", nameof(customHtmlContent));
        }

        return normalized;
    }

    private void ApplyValidityWindow(DateTimeOffset? validFromUtc, DateTimeOffset? validUntilUtc)
    {
        if (validFromUtc.HasValue && validUntilUtc.HasValue && validUntilUtc.Value < validFromUtc.Value)
        {
            throw new ArgumentException("Tracking page validity end date must be greater than or equal to start date.");
        }

        ValidFromUtc = validFromUtc;
        ValidUntilUtc = validUntilUtc;
    }

}
