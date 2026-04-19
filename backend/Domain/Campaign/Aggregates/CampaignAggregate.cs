using System;
using System.Collections.Generic;

using QPhising.Domain.Campaign.Enums;
using QPhising.Domain.Campaign.Policies;
using QPhising.Domain.Campaign.ValueObjects;
using QPhising.Domain.Common;

namespace QPhising.Domain.Campaign.Aggregates;

public sealed class CampaignAggregate : Entity<Guid>
{
    public CampaignAggregate(Guid id, CampaignName name, Guid templateId)
        : this(
            id,
            name,
            templateId,
            CampaignLifecycleState.Draft,
            scheduleWindow: null,
            createdAtUtc: DateTimeOffset.UtcNow,
            updatedAtUtc: null)
    {
    }

    private CampaignAggregate(
        Guid id,
        CampaignName name,
        Guid templateId,
        CampaignLifecycleState lifecycleState,
        CampaignScheduleWindow? scheduleWindow,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? updatedAtUtc)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (templateId == Guid.Empty)
        {
            throw new ArgumentException("Template ID is required.", nameof(templateId));
        }

        Name = name;
        TemplateId = templateId;
        LifecycleState = lifecycleState;
        ScheduleWindow = scheduleWindow;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc ?? createdAtUtc;
    }

    public CampaignName Name { get; private set; }

    public Guid TemplateId { get; private set; }

    public CampaignLifecycleState LifecycleState { get; private set; }

    public CampaignScheduleWindow? ScheduleWindow { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

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
        Guid templateId,
        CampaignLifecycleState lifecycleState,
        CampaignScheduleWindow? scheduleWindow,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc)
    {
        return new CampaignAggregate(
            id,
            name,
            templateId,
            lifecycleState,
            scheduleWindow,
            createdAtUtc,
            updatedAtUtc);
    }

    private void TransitionTo(CampaignLifecycleState nextState)
    {
        CampaignLifecyclePolicy.EnsureTransitionAllowed(LifecycleState, nextState);
        LifecycleState = nextState;
        Touch();
    }

    private void EnsureMutable()
    {
        if (LifecycleState is CampaignLifecycleState.Completed or CampaignLifecycleState.Cancelled)
        {
            throw new InvalidOperationException("Completed or cancelled campaigns cannot be modified.");
        }
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
