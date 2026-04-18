using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using QPhising.Domain.Campaign.Entities;
using QPhising.Domain.Campaign.Enums;
using QPhising.Domain.Campaign.Policies;
using QPhising.Domain.Campaign.ValueObjects;
using QPhising.Domain.Common;

namespace QPhising.Domain.Campaign.Aggregates;

public sealed class CampaignAggregate : Entity<Guid>
{
    private readonly List<CampaignTarget> _targets = [];

    public CampaignAggregate(Guid id, CampaignName name, Guid templateId)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (templateId == Guid.Empty)
        {
            throw new ArgumentException("Template ID is required.", nameof(templateId));
        }

        Name = name;
        TemplateId = templateId;
        LifecycleState = CampaignLifecycleState.Draft;
        CreatedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = CreatedAtUtc;
    }

    public CampaignName Name { get; private set; }

    public Guid TemplateId { get; private set; }

    public CampaignLifecycleState LifecycleState { get; private set; }

    public CampaignScheduleWindow? ScheduleWindow { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public IReadOnlyCollection<CampaignTarget> Targets => new ReadOnlyCollection<CampaignTarget>(_targets);

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

    public void AddTarget(CampaignTarget target)
    {
        ArgumentNullException.ThrowIfNull(target);

        EnsureMutable();

        if (_targets.Any(existing => string.Equals(existing.EmailAddress, target.EmailAddress, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Target '{target.EmailAddress}' is already part of this campaign.");
        }

        _targets.Add(target);
        Touch();
    }

    public void RemoveTarget(Guid targetId)
    {
        EnsureMutable();

        var target = _targets.FirstOrDefault(existing => existing.Id == targetId)
            ?? throw new InvalidOperationException("Target does not exist in this campaign.");

        _targets.Remove(target);
        Touch();
    }

    public void Schedule()
    {
        if (ScheduleWindow is null)
        {
            throw new InvalidOperationException("Campaign schedule is required before scheduling the campaign.");
        }

        if (_targets.Count == 0)
        {
            throw new InvalidOperationException("Campaign must have at least one target before scheduling.");
        }

        TransitionTo(CampaignLifecycleState.Scheduled);
    }

    public void Start() => TransitionTo(CampaignLifecycleState.Active);

    public void Pause() => TransitionTo(CampaignLifecycleState.Paused);

    public void Complete() => TransitionTo(CampaignLifecycleState.Completed);

    public void Cancel() => TransitionTo(CampaignLifecycleState.Cancelled);

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
