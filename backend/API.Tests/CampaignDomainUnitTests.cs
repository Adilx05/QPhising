using QPhising.Domain.Campaign.Aggregates;
using QPhising.Domain.Campaign.Enums;
using QPhising.Domain.Campaign.Policies;
using QPhising.Domain.Campaign.ValueObjects;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class CampaignDomainUnitTests
{
    [Fact]
    public void Constructor_ShouldCreateInDraftState()
    {
        var campaign = CreateCampaign();

        Assert.Equal(CampaignLifecycleState.Draft, campaign.LifecycleState);
        Assert.Null(campaign.ScheduleWindow);
    }

    [Fact]
    public void Constructor_ShouldSetPropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var name = new CampaignName("Test Campaign");
        var trackingPageId = Guid.NewGuid();
        var templateId = Guid.NewGuid();

        var campaign = new CampaignAggregate(id, name, trackingPageId, templateId);

        Assert.Equal(id, campaign.Id);
        Assert.Equal("Test Campaign", campaign.Name.Value);
        Assert.Equal(trackingPageId, campaign.TrackingPageId);
        Assert.Equal(templateId, campaign.TemplateId);
    }

    [Fact]
    public void Constructor_ShouldThrowWhenTrackingPageIdIsEmpty()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new CampaignAggregate(Guid.NewGuid(), new CampaignName("Test"), Guid.Empty, null));

        Assert.Contains("trackingPageId", ex.Message);
    }

    [Fact]
    public void Constructor_ShouldNormalizeTemplateIdEmptyToNull()
    {
        var campaign = CreateCampaign(templateId: Guid.Empty);

        Assert.Null(campaign.TemplateId);
    }

    [Fact]
    public void Constructor_ShouldKeepNonNullTemplateId()
    {
        var templateId = Guid.NewGuid();
        var campaign = CreateCampaign(templateId: templateId);

        Assert.Equal(templateId, campaign.TemplateId);
    }

    [Fact]
    public void Rename_ShouldUpdateName()
    {
        var campaign = CreateCampaign();
        var newName = new CampaignName("Renamed Campaign");

        campaign.Rename(newName);

        Assert.Equal("Renamed Campaign", campaign.Name.Value);
    }

    [Fact]
    public void Rename_ShouldThrowForNull()
    {
        var campaign = CreateCampaign();

        Assert.Throws<ArgumentNullException>(() => campaign.Rename(null!));
    }

    [Fact]
    public void SetSchedule_ShouldStoreScheduleWindow()
    {
        var campaign = CreateCampaign();
        var window = new CampaignScheduleWindow(
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2));

        campaign.SetSchedule(window);

        Assert.NotNull(campaign.ScheduleWindow);
        Assert.Equal(window.StartsAtUtc, campaign.ScheduleWindow!.StartsAtUtc);
        Assert.Equal(window.EndsAtUtc, campaign.ScheduleWindow.EndsAtUtc);
    }

    [Fact]
    public void SetSchedule_ShouldThrowForNull()
    {
        var campaign = CreateCampaign();

        Assert.Throws<ArgumentNullException>(() => campaign.SetSchedule(null!));
    }

    [Fact]
    public void Schedule_ShouldTransitionFromDraftToScheduled()
    {
        var campaign = CreateCampaign();
        campaign.SetSchedule(new CampaignScheduleWindow(
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2)));

        campaign.Schedule();

        Assert.Equal(CampaignLifecycleState.Scheduled, campaign.LifecycleState);
    }

    [Fact]
    public void Schedule_ShouldThrowWhenNoScheduleWindow()
    {
        var campaign = CreateCampaign();

        var ex = Assert.Throws<InvalidOperationException>(() => campaign.Schedule());
        Assert.Contains("schedule is required", ex.Message);
    }

    [Fact]
    public void Start_ShouldTransitionFromDraftToActive()
    {
        var campaign = CreateCampaign();

        campaign.Start();

        Assert.Equal(CampaignLifecycleState.Active, campaign.LifecycleState);
    }

    [Fact]
    public void Start_ShouldTransitionFromScheduledToActive()
    {
        var campaign = CreateCampaign();
        campaign.SetSchedule(new CampaignScheduleWindow(
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2)));
        campaign.Schedule();

        campaign.Start();

        Assert.Equal(CampaignLifecycleState.Active, campaign.LifecycleState);
    }

    [Fact]
    public void Start_ShouldTransitionFromPausedToActive()
    {
        var campaign = CreateCampaign();
        campaign.Start();
        campaign.Pause();

        campaign.Start();

        Assert.Equal(CampaignLifecycleState.Active, campaign.LifecycleState);
    }

    [Fact]
    public void Start_ShouldNotThrowWhenAlreadyActive()
    {
        var campaign = CreateCampaign();
        campaign.Start();

        campaign.Start();

        Assert.Equal(CampaignLifecycleState.Active, campaign.LifecycleState);
    }

    [Fact]
    public void Start_ShouldThrowWhenCompleted()
    {
        var campaign = CreateCampaign();
        campaign.Start();
        campaign.Complete();

        Assert.Throws<InvalidOperationException>(() => campaign.Start());
    }

    [Fact]
    public void Start_ShouldThrowWhenCancelled()
    {
        var campaign = CreateCampaign();
        campaign.Start();
        campaign.Cancel();

        Assert.Throws<InvalidOperationException>(() => campaign.Start());
    }

    [Fact]
    public void Pause_ShouldTransitionFromActiveToPaused()
    {
        var campaign = CreateCampaign();
        campaign.Start();

        campaign.Pause();

        Assert.Equal(CampaignLifecycleState.Paused, campaign.LifecycleState);
    }

    [Fact]
    public void Pause_ShouldThrowFromNonActiveStates()
    {
        var campaign = CreateCampaign();

        Assert.Throws<InvalidOperationException>(() => campaign.Pause());
    }

    [Fact]
    public void Complete_ShouldTransitionFromActiveToCompleted()
    {
        var campaign = CreateCampaign();
        campaign.Start();

        campaign.Complete();

        Assert.Equal(CampaignLifecycleState.Completed, campaign.LifecycleState);
    }

    [Fact]
    public void Complete_ShouldThrowFromNonActiveStates()
    {
        var campaign = CreateCampaign();

        Assert.Throws<InvalidOperationException>(() => campaign.Complete());
    }

    [Fact]
    public void Cancel_ShouldTransitionFromDraftToCancelled()
    {
        var campaign = CreateCampaign();

        campaign.Cancel();

        Assert.Equal(CampaignLifecycleState.Cancelled, campaign.LifecycleState);
    }

    [Fact]
    public void Cancel_ShouldTransitionFromScheduledToCancelled()
    {
        var campaign = CreateCampaign();
        campaign.SetSchedule(new CampaignScheduleWindow(
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(2)));
        campaign.Schedule();

        campaign.Cancel();

        Assert.Equal(CampaignLifecycleState.Cancelled, campaign.LifecycleState);
    }

    [Fact]
    public void Cancel_ShouldTransitionFromActiveToCancelled()
    {
        var campaign = CreateCampaign();
        campaign.Start();

        campaign.Cancel();

        Assert.Equal(CampaignLifecycleState.Cancelled, campaign.LifecycleState);
    }

    [Fact]
    public void Cancel_ShouldTransitionFromPausedToCancelled()
    {
        var campaign = CreateCampaign();
        campaign.Start();
        campaign.Pause();

        campaign.Cancel();

        Assert.Equal(CampaignLifecycleState.Cancelled, campaign.LifecycleState);
    }

    [Fact]
    public void Cancel_ShouldThrowWhenCompleted()
    {
        var campaign = CreateCampaign();
        campaign.Start();
        campaign.Complete();

        Assert.Throws<InvalidOperationException>(() => campaign.Cancel());
    }

    [Fact]
    public void Cancel_ShouldNotThrowWhenAlreadyCancelled()
    {
        var campaign = CreateCampaign();
        campaign.Cancel();

        campaign.Cancel();

        Assert.Equal(CampaignLifecycleState.Cancelled, campaign.LifecycleState);
    }

    [Fact]
    public void EnsureMutable_ShouldBlockModificationAfterComplete()
    {
        var campaign = CreateCampaign();
        campaign.Start();
        campaign.Complete();

        Assert.Throws<InvalidOperationException>(() => campaign.Rename(new CampaignName("Should Fail")));
        Assert.Throws<InvalidOperationException>(() => campaign.SetSchedule(
            new CampaignScheduleWindow(DateTimeOffset.UtcNow.AddDays(1), DateTimeOffset.UtcNow.AddDays(2))));
    }

    [Fact]
    public void EnsureMutable_ShouldBlockModificationAfterCancel()
    {
        var campaign = CreateCampaign();
        campaign.Cancel();

        Assert.Throws<InvalidOperationException>(() => campaign.Rename(new CampaignName("Should Fail")));
    }

    [Fact]
    public void EnsureMutable_ShouldBlockModificationAfterDelete()
    {
        var campaign = CreateCampaign();
        campaign.MarkDeleted();

        Assert.Throws<InvalidOperationException>(() => campaign.Rename(new CampaignName("Should Fail")));
    }

    [Fact]
    public void CampaignName_ShouldNormalizeWhitespace()
    {
        var name = new CampaignName("  My Campaign  ");

        Assert.Equal("My Campaign", name.Value);
    }

    [Fact]
    public void CampaignName_ShouldRejectNull()
    {
        Assert.Throws<ArgumentException>(() => new CampaignName(null!));
    }

    [Fact]
    public void CampaignName_ShouldRejectEmpty()
    {
        Assert.Throws<ArgumentException>(() => new CampaignName(""));
    }

    [Fact]
    public void CampaignName_ShouldRejectWhitespaceOnly()
    {
        Assert.Throws<ArgumentException>(() => new CampaignName("   "));
    }

    [Fact]
    public void CampaignName_ShouldRejectExceedingMaxLength()
    {
        var longName = new string('x', CampaignName.MaxLength + 1);

        Assert.Throws<ArgumentException>(() => new CampaignName(longName));
    }

    [Fact]
    public void CampaignName_ShouldAcceptAtMaxLength()
    {
        var validName = new string('x', CampaignName.MaxLength);

        var name = new CampaignName(validName);

        Assert.Equal(validName, name.Value);
    }

    [Fact]
    public void CampaignName_Equality_ShouldBeBasedOnValue()
    {
        var name1 = new CampaignName("Campaign A");
        var name2 = new CampaignName("Campaign A");
        var name3 = new CampaignName("Campaign B");

        Assert.Equal(name1, name2);
        Assert.NotEqual(name1, name3);
        Assert.Equal(name1.GetHashCode(), name2.GetHashCode());
    }

    [Fact]
    public void CampaignScheduleWindow_ShouldThrowWhenEndEqualsStart()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);

        Assert.Throws<ArgumentException>(() =>
            new CampaignScheduleWindow(start, start));
    }

    [Fact]
    public void CampaignScheduleWindow_ShouldThrowWhenEndBeforeStart()
    {
        var start = DateTimeOffset.UtcNow.AddDays(2);

        Assert.Throws<ArgumentException>(() =>
            new CampaignScheduleWindow(start, DateTimeOffset.UtcNow.AddDays(1)));
    }

    [Fact]
    public void CampaignScheduleWindow_ShouldAcceptNullEnd()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);

        var window = new CampaignScheduleWindow(start, null);

        Assert.Equal(start, window.StartsAtUtc);
        Assert.Null(window.EndsAtUtc);
    }

    [Fact]
    public void CampaignScheduleWindow_ShouldThrowWhenStartIsMinValue()
    {
        Assert.Throws<ArgumentException>(() =>
            new CampaignScheduleWindow(DateTimeOffset.MinValue, null));
    }

    [Fact]
    public void CampaignScheduleWindow_Equality_ShouldBeBasedOnStartAndEnd()
    {
        var start = DateTimeOffset.UtcNow.AddDays(1);
        var end = DateTimeOffset.UtcNow.AddDays(2);
        var w1 = new CampaignScheduleWindow(start, end);
        var w2 = new CampaignScheduleWindow(start, end);
        var w3 = new CampaignScheduleWindow(start, null);

        Assert.Equal(w1, w2);
        Assert.NotEqual(w1, w3);
        Assert.Equal(w1.GetHashCode(), w2.GetHashCode());
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Draft_ShouldAllowScheduled()
    {
        Assert.True(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Draft, CampaignLifecycleState.Scheduled));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Draft_ShouldAllowActive()
    {
        Assert.True(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Draft, CampaignLifecycleState.Active));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Draft_ShouldAllowCancelled()
    {
        Assert.True(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Draft, CampaignLifecycleState.Cancelled));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Draft_ShouldNotAllowPaused()
    {
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Draft, CampaignLifecycleState.Paused));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Draft_ShouldNotAllowCompleted()
    {
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Draft, CampaignLifecycleState.Completed));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Scheduled_ShouldAllowActive()
    {
        Assert.True(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Scheduled, CampaignLifecycleState.Active));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Scheduled_ShouldAllowCancelled()
    {
        Assert.True(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Scheduled, CampaignLifecycleState.Cancelled));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Scheduled_ShouldNotAllowPaused()
    {
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Scheduled, CampaignLifecycleState.Paused));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Scheduled_ShouldNotAllowCompleted()
    {
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Scheduled, CampaignLifecycleState.Completed));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Active_ShouldAllowPaused()
    {
        Assert.True(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Active, CampaignLifecycleState.Paused));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Active_ShouldAllowCompleted()
    {
        Assert.True(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Active, CampaignLifecycleState.Completed));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Active_ShouldAllowCancelled()
    {
        Assert.True(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Active, CampaignLifecycleState.Cancelled));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Active_ShouldNotAllowDraft()
    {
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Active, CampaignLifecycleState.Draft));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Paused_ShouldAllowActive()
    {
        Assert.True(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Paused, CampaignLifecycleState.Active));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Paused_ShouldAllowCancelled()
    {
        Assert.True(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Paused, CampaignLifecycleState.Cancelled));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Paused_ShouldNotAllowCompleted()
    {
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Paused, CampaignLifecycleState.Completed));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Completed_ShouldNotAllowAny()
    {
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Completed, CampaignLifecycleState.Draft));
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Completed, CampaignLifecycleState.Scheduled));
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Completed, CampaignLifecycleState.Active));
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Completed, CampaignLifecycleState.Paused));
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Completed, CampaignLifecycleState.Cancelled));
    }

    [Fact]
    public void CampaignLifecyclePolicy_CanTransition_Cancelled_ShouldNotAllowAny()
    {
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Cancelled, CampaignLifecycleState.Draft));
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Cancelled, CampaignLifecycleState.Scheduled));
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Cancelled, CampaignLifecycleState.Active));
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Cancelled, CampaignLifecycleState.Paused));
        Assert.False(CampaignLifecyclePolicy.CanTransition(CampaignLifecycleState.Cancelled, CampaignLifecycleState.Completed));
    }

    [Fact]
    public void CampaignLifecyclePolicy_EnsureTransitionAllowed_ShouldNotThrowForSameState()
    {
        CampaignLifecyclePolicy.EnsureTransitionAllowed(CampaignLifecycleState.Draft, CampaignLifecycleState.Draft);
    }

    [Fact]
    public void CampaignLifecyclePolicy_EnsureTransitionAllowed_ShouldThrowForInvalid()
    {
        Assert.Throws<InvalidOperationException>(() =>
            CampaignLifecyclePolicy.EnsureTransitionAllowed(CampaignLifecycleState.Draft, CampaignLifecycleState.Completed));
    }

    [Fact]
    public void MarkDeleted_ShouldSetIsDeleted()
    {
        var campaign = CreateCampaign();

        campaign.MarkDeleted();

        Assert.True(campaign.IsDeleted);
    }

    private static CampaignAggregate CreateCampaign(Guid? templateId = null)
        => new(
            Guid.NewGuid(),
            new CampaignName("Test Campaign"),
            Guid.NewGuid(),
            templateId);
}
