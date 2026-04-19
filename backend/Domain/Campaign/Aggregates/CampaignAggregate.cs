using System;
using System.Collections.Generic;

using QPhising.Domain.Campaign.Enums;
using QPhising.Domain.Campaign.Policies;
using QPhising.Domain.Campaign.ValueObjects;
using QPhising.Domain.Common;

namespace QPhising.Domain.Campaign.Aggregates;

public sealed class CampaignAggregate : AuditableSoftDeletableEntity<Guid>
{
    public CampaignAggregate(Guid id, CampaignName name, Guid trackingPageId, Guid? templateId)
        : this(
            id,
            name,
            trackingPageId,
            templateId,
            CampaignLifecycleState.Draft,
            scheduleWindow: null,
            createdAtUtc: DateTimeOffset.UtcNow,
            updatedAtUtc: null,
            isDeleted: false,
            deletedAtUtc: null,
            deletedBy: null)
    {
    }

    private CampaignAggregate(
        Guid id,
        CampaignName name,
        Guid trackingPageId,
        Guid? templateId,
        CampaignLifecycleState lifecycleState,
        CampaignScheduleWindow? scheduleWindow,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc,
        bool isDeleted,
        DateTimeOffset? deletedAtUtc,
        string? deletedBy)
        : base(id, createdAtUtc, updatedAtUtc, isDeleted, deletedAtUtc, deletedBy)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (trackingPageId == Guid.Empty)
        {
            throw new ArgumentException("Tracking page ID is required.", nameof(trackingPageId));
        }

        Name = name;
        TrackingPageId = trackingPageId;
        TemplateId = NormalizeTemplateId(templateId);
        LifecycleState = lifecycleState;
        ScheduleWindow = scheduleWindow;
    }

    public CampaignName Name { get; private set; }

    public Guid TrackingPageId { get; private set; }

    public Guid? TemplateId { get; private set; }

    public CampaignLifecycleState LifecycleState { get; private set; }

    public CampaignScheduleWindow? ScheduleWindow { get; private set; }

    public void Rename(CampaignName name)
    {
        ArgumentNullException.ThrowIfNull(name);

        EnsureMutable();

        Name = name;
        Touch();
    }

    public void SetSchedule(CampaignScheduleWindow scheduleWindow)
    {
        ArgumentNullException.ThrowIfNull(scheduleWindow);

        EnsureMutable();

        ScheduleWindow = scheduleWindow;
        Touch();
    }

    public void Schedule()
    {
        if (ScheduleWindow is null)
        {
            throw new InvalidOperationException("Campaign schedule is required before scheduling the campaign.");
        }

        TransitionTo(CampaignLifecycleState.Scheduled);
    }

    public void Start() => TransitionTo(CampaignLifecycleState.Active);

    public void Pause() => TransitionTo(CampaignLifecycleState.Paused);

    public void Complete() => TransitionTo(CampaignLifecycleState.Completed);

    public void Cancel() => TransitionTo(CampaignLifecycleState.Cancelled);

    public static CampaignAggregate Rehydrate(
        Guid id,
        CampaignName name,
        Guid trackingPageId,
        Guid? templateId,
        CampaignLifecycleState lifecycleState,
        CampaignScheduleWindow? scheduleWindow,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        bool isDeleted = false,
        DateTimeOffset? deletedAtUtc = null,
        string? deletedBy = null)
    {
        return new CampaignAggregate(
            id,
            name,
            trackingPageId,
            templateId,
            lifecycleState,
            scheduleWindow,
            createdAtUtc,
            updatedAtUtc,
            isDeleted,
            deletedAtUtc,
            deletedBy);
    }

    private void TransitionTo(CampaignLifecycleState nextState)
    {
        CampaignLifecyclePolicy.EnsureTransitionAllowed(LifecycleState, nextState);
        LifecycleState = nextState;
        Touch();
    }

    private void EnsureMutable()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Deleted campaigns cannot be modified.");
        }

        if (LifecycleState is CampaignLifecycleState.Completed or CampaignLifecycleState.Cancelled)
        {
            throw new InvalidOperationException("Completed or cancelled campaigns cannot be modified.");
        }
    }

    private static Guid? NormalizeTemplateId(Guid? templateId)
    {
        if (!templateId.HasValue || templateId.Value == Guid.Empty)
        {
            return null;
        }

        return templateId.Value;
    }
}
