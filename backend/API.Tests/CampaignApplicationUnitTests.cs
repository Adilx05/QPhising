using QPhising.Application.Contracts.Abstractions.Authorization;
using QPhising.Application.Contracts.Abstractions.Campaign;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.CQRS.Commands.Campaign;
using QPhising.Application.CQRS.Queries.Campaign;
using QPhising.Domain.Campaign.Aggregates;
using QPhising.Domain.Campaign.Enums;
using QPhising.Domain.Campaign.ValueObjects;
using QPhising.Domain.Tracking.Aggregates;
using QPhising.Domain.Tracking.Enums;
using QPhising.Domain.Tracking.Models;
using QPhising.Domain.Tracking.ValueObjects;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class CampaignApplicationUnitTests
{
    [Fact]
    public async Task CreateCampaignCommandHandler_ShouldCreateTrackingPageAndCampaign()
    {
        var slug = $"test-page-{Guid.NewGuid():N}";
        var command = new CreateCampaignCommand(
            Name: "Test Campaign",
            TrackingPageSlug: slug,
            TrackingPageTitle: "Test Page",
            TrackingPageDescription: null,
            TemplateId: null,
            HtmlContent: null,
            ValidFromUtc: null,
            ValidUntilUtc: null);

        var trackingRepo = new FakeTrackingPageRepository();
        var campaignRepo = new FakeCampaignRepository();
        var currentUser = new FakeCurrentUserContext();
        var handler = new CreateCampaignCommandHandler(campaignRepo, trackingRepo, currentUser);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("Test Campaign", result.Name);
        Assert.Equal(command.TrackingPageSlug, trackingRepo.SavedAggregate?.Slug.Value);
        Assert.Equal("Test Campaign", campaignRepo.SavedCampaign?.Name.Value);
        Assert.Equal(trackingRepo.SavedAggregate?.Id, campaignRepo.SavedCampaign?.TrackingPageId);
        Assert.Equal(CampaignLifecycleState.Draft, result.LifecycleState);
    }

    [Fact]
    public async Task CreateCampaignCommandHandler_ShouldThrowOnSlugConflict()
    {
        var slug = $"duplicate-{Guid.NewGuid():N}";
        var command = new CreateCampaignCommand(
            Name: "Duplicate",
            TrackingPageSlug: slug,
            TrackingPageTitle: "Page",
            TrackingPageDescription: null,
            TemplateId: null,
            HtmlContent: null,
            ValidFromUtc: null,
            ValidUntilUtc: null);

        var trackingRepo = new FakeTrackingPageRepository { SlugExistsResult = true };
        var campaignRepo = new FakeCampaignRepository();
        var currentUser = new FakeCurrentUserContext();
        var handler = new CreateCampaignCommandHandler(campaignRepo, trackingRepo, currentUser);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));

        Assert.Contains("already in use", ex.Message);
        Assert.Null(campaignRepo.SavedCampaign);
    }

    [Fact]
    public async Task UpdateCampaignCommandHandler_ShouldRenameAndSetSchedule()
    {
        var campaign = new CampaignAggregate(
            Guid.NewGuid(),
            new CampaignName("Original"),
            Guid.NewGuid(),
            templateId: null);
        var repo = new FakeCampaignRepository(campaign);

        var handler = new UpdateCampaignCommandHandler(repo);
        var startsAt = DateTimeOffset.UtcNow.AddDays(1);

        var result = await handler.Handle(
            new UpdateCampaignCommand(campaign.Id, "Updated Name", startsAt, null),
            CancellationToken.None);

        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(startsAt, result.StartsAtUtc);
        Assert.NotNull(repo.SavedCampaign);
        Assert.Equal("Updated Name", repo.SavedCampaign!.Name.Value);
    }

    [Fact]
    public async Task UpdateCampaignCommandHandler_ShouldThrowWhenNotFound()
    {
        var repo = new FakeCampaignRepository(null as CampaignAggregate);
        var handler = new UpdateCampaignCommandHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(
                new UpdateCampaignCommand(Guid.NewGuid(), "Name", null, null),
                CancellationToken.None));
    }

    [Fact]
    public async Task DeleteCampaignCommandHandler_ShouldSoftDeleteCampaignAndTrackingPage()
    {
        var trackingPage = CreateTrackingPage("delete-campaign");
        var campaign = new CampaignAggregate(
            Guid.NewGuid(),
            new CampaignName("To Delete"),
            trackingPage.Id,
            templateId: null);

        var trackingRepo = new FakeTrackingPageRepository(trackingPage);
        var campaignRepo = new FakeCampaignRepository(campaign);
        var handler = new DeleteCampaignCommandHandler(campaignRepo, trackingRepo);

        await handler.Handle(new DeleteCampaignCommand(campaign.Id), CancellationToken.None);

        Assert.NotNull(campaignRepo.DeletedAggregate);
        Assert.True(campaignRepo.DeletedAggregate!.IsDeleted);
        Assert.NotNull(trackingRepo.DeletedAggregate);
        Assert.True(trackingRepo.DeletedAggregate!.IsDeleted);
    }

    [Fact]
    public async Task DeleteCampaignCommandHandler_ShouldThrowWhenCampaignNotFound()
    {
        var trackingRepo = new FakeTrackingPageRepository();
        var campaignRepo = new FakeCampaignRepository(null as CampaignAggregate);
        var handler = new DeleteCampaignCommandHandler(campaignRepo, trackingRepo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new DeleteCampaignCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task StartCampaignCommandHandler_ShouldTransitionToActiveAndPublishTrackingPage()
    {
        var trackingPage = CreateTrackingPage("start-campaign");
        var campaign = new CampaignAggregate(
            Guid.NewGuid(),
            new CampaignName("Start Test"),
            trackingPage.Id,
            templateId: null);

        var trackingRepo = new FakeTrackingPageRepository(trackingPage);
        var campaignRepo = new FakeCampaignRepository(campaign);
        var handler = new StartCampaignCommandHandler(campaignRepo, trackingRepo);

        var result = await handler.Handle(new StartCampaignCommand(campaign.Id), CancellationToken.None);

        Assert.Equal(CampaignLifecycleState.Active, result.LifecycleState);
        Assert.NotNull(trackingRepo.SavedAggregate);
        Assert.Equal(TrackingPagePublishState.Published, trackingRepo.SavedAggregate!.PublishState);
        Assert.NotNull(campaignRepo.SavedCampaign);
    }

    [Fact]
    public async Task StartCampaignCommandHandler_ShouldNotRepublishAlreadyPublishedTrackingPage()
    {
        var trackingPage = CreateTrackingPage("already-published");
        trackingPage.Publish();
        var campaign = new CampaignAggregate(
            Guid.NewGuid(),
            new CampaignName("Start Published"),
            trackingPage.Id,
            templateId: null);

        var trackingRepo = new FakeTrackingPageRepository(trackingPage);
        var campaignRepo = new FakeCampaignRepository(campaign);
        var handler = new StartCampaignCommandHandler(campaignRepo, trackingRepo);

        var result = await handler.Handle(new StartCampaignCommand(campaign.Id), CancellationToken.None);

        Assert.Equal(CampaignLifecycleState.Active, result.LifecycleState);
        Assert.Null(trackingRepo.SavedAggregate);
    }

    [Fact]
    public async Task StartCampaignCommandHandler_ShouldThrowWhenCampaignNotFound()
    {
        var trackingRepo = new FakeTrackingPageRepository();
        var campaignRepo = new FakeCampaignRepository(null as CampaignAggregate);
        var handler = new StartCampaignCommandHandler(campaignRepo, trackingRepo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new StartCampaignCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task PauseCampaignCommandHandler_ShouldTransitionToPaused()
    {
        var campaign = new CampaignAggregate(
            Guid.NewGuid(),
            new CampaignName("Pause Test"),
            Guid.NewGuid(),
            templateId: null);
        campaign.Start();
        var repo = new FakeCampaignRepository(campaign);
        var handler = new PauseCampaignCommandHandler(repo);

        var result = await handler.Handle(new PauseCampaignCommand(campaign.Id), CancellationToken.None);

        Assert.Equal(CampaignLifecycleState.Paused, result.LifecycleState);
        Assert.NotNull(repo.SavedCampaign);
    }

    [Fact]
    public async Task PauseCampaignCommandHandler_ShouldThrowWhenCampaignNotFound()
    {
        var repo = new FakeCampaignRepository(null as CampaignAggregate);
        var handler = new PauseCampaignCommandHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new PauseCampaignCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task CompleteCampaignCommandHandler_ShouldTransitionToCompleted()
    {
        var campaign = new CampaignAggregate(
            Guid.NewGuid(),
            new CampaignName("Complete Test"),
            Guid.NewGuid(),
            templateId: null);
        campaign.Start();
        var repo = new FakeCampaignRepository(campaign);
        var handler = new CompleteCampaignCommandHandler(repo);

        var result = await handler.Handle(new CompleteCampaignCommand(campaign.Id), CancellationToken.None);

        Assert.Equal(CampaignLifecycleState.Completed, result.LifecycleState);
        Assert.NotNull(repo.SavedCampaign);
    }

    [Fact]
    public async Task CompleteCampaignCommandHandler_ShouldThrowWhenCampaignNotFound()
    {
        var repo = new FakeCampaignRepository(null as CampaignAggregate);
        var handler = new CompleteCampaignCommandHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new CompleteCampaignCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task CancelCampaignCommandHandler_ShouldTransitionToCancelled()
    {
        var campaign = new CampaignAggregate(
            Guid.NewGuid(),
            new CampaignName("Cancel Test"),
            Guid.NewGuid(),
            templateId: null);
        campaign.Start();
        var repo = new FakeCampaignRepository(campaign);
        var handler = new CancelCampaignCommandHandler(repo);

        var result = await handler.Handle(new CancelCampaignCommand(campaign.Id), CancellationToken.None);

        Assert.Equal(CampaignLifecycleState.Cancelled, result.LifecycleState);
        Assert.NotNull(repo.SavedCampaign);
    }

    [Fact]
    public async Task CancelCampaignCommandHandler_ShouldThrowWhenCampaignNotFound()
    {
        var repo = new FakeCampaignRepository(null as CampaignAggregate);
        var handler = new CancelCampaignCommandHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new CancelCampaignCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ScheduleCampaignCommandHandler_ShouldSetScheduleAndTransitionToScheduled()
    {
        var campaign = new CampaignAggregate(
            Guid.NewGuid(),
            new CampaignName("Schedule Test"),
            Guid.NewGuid(),
            templateId: null);
        var repo = new FakeCampaignRepository(campaign);
        var handler = new ScheduleCampaignCommandHandler(repo);
        var startsAt = DateTimeOffset.UtcNow.AddDays(1);

        var result = await handler.Handle(
            new ScheduleCampaignCommand(campaign.Id, startsAt, startsAt.AddDays(1)),
            CancellationToken.None);

        Assert.Equal(CampaignLifecycleState.Scheduled, result.LifecycleState);
        Assert.Equal(startsAt, result.StartsAtUtc);
        Assert.NotNull(repo.SavedCampaign);
    }

    [Fact]
    public async Task ScheduleCampaignCommandHandler_ShouldThrowWhenCampaignNotFound()
    {
        var repo = new FakeCampaignRepository(null as CampaignAggregate);
        var handler = new ScheduleCampaignCommandHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(
                new ScheduleCampaignCommand(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), null),
                CancellationToken.None));
    }

    [Fact]
    public async Task ListCampaignsQueryHandler_ShouldReturnAllCampaigns()
    {
        var campaign = new CampaignAggregate(
            Guid.NewGuid(),
            new CampaignName("List Test"),
            Guid.NewGuid(),
            templateId: null);
        var repo = new FakeCampaignRepository(campaign);
        var handler = new ListCampaignsQueryHandler(repo);

        var results = await handler.Handle(new ListCampaignsQuery(), CancellationToken.None);

        Assert.Single(results);
        Assert.Equal("List Test", results.First().Name);
    }

    [Fact]
    public async Task GetCampaignByIdQueryHandler_ShouldReturnCampaign()
    {
        var campaign = new CampaignAggregate(
            Guid.NewGuid(),
            new CampaignName("Get By Id"),
            Guid.NewGuid(),
            templateId: null);
        var repo = new FakeCampaignRepository(campaign);
        var handler = new GetCampaignByIdQueryHandler(repo);

        var result = await handler.Handle(new GetCampaignByIdQuery(campaign.Id), CancellationToken.None);

        Assert.Equal(campaign.Id, result.Id);
        Assert.Equal("Get By Id", result.Name);
    }

    [Fact]
    public async Task GetCampaignByIdQueryHandler_ShouldThrowWhenNotFound()
    {
        var repo = new FakeCampaignRepository(null as CampaignAggregate);
        var handler = new GetCampaignByIdQueryHandler(repo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new GetCampaignByIdQuery(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public void CreateCampaignCommandValidator_ShouldAcceptValidCommand()
    {
        var validator = new CreateCampaignCommandValidator();
        var command = new CreateCampaignCommand(
            Name: "Valid Campaign",
            TrackingPageSlug: "valid-slug",
            TrackingPageTitle: "Valid Title",
            TrackingPageDescription: null,
            TemplateId: null,
            HtmlContent: null,
            ValidFromUtc: null,
            ValidUntilUtc: null);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateCampaignCommandValidator_ShouldRejectEmptyName()
    {
        var validator = new CreateCampaignCommandValidator();
        var command = new CreateCampaignCommand(
            Name: "",
            TrackingPageSlug: "valid-slug",
            TrackingPageTitle: "Valid Title",
            TrackingPageDescription: null,
            TemplateId: null,
            HtmlContent: null,
            ValidFromUtc: null,
            ValidUntilUtc: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCampaignCommand.Name));
    }

    [Fact]
    public void CreateCampaignCommandValidator_ShouldRejectNameExceedingMaxLength()
    {
        var validator = new CreateCampaignCommandValidator();
        var command = new CreateCampaignCommand(
            Name: new string('x', CampaignName.MaxLength + 1),
            TrackingPageSlug: "valid-slug",
            TrackingPageTitle: "Valid Title",
            TrackingPageDescription: null,
            TemplateId: null,
            HtmlContent: null,
            ValidFromUtc: null,
            ValidUntilUtc: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCampaignCommand.Name));
    }

    [Fact]
    public void CreateCampaignCommandValidator_ShouldRejectEmptySlug()
    {
        var validator = new CreateCampaignCommandValidator();
        var command = new CreateCampaignCommand(
            Name: "Campaign",
            TrackingPageSlug: "",
            TrackingPageTitle: "Title",
            TrackingPageDescription: null,
            TemplateId: null,
            HtmlContent: null,
            ValidFromUtc: null,
            ValidUntilUtc: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCampaignCommand.TrackingPageSlug));
    }

    [Fact]
    public void CreateCampaignCommandValidator_ShouldRejectEmptyTitle()
    {
        var validator = new CreateCampaignCommandValidator();
        var command = new CreateCampaignCommand(
            Name: "Campaign",
            TrackingPageSlug: "valid-slug",
            TrackingPageTitle: "",
            TrackingPageDescription: null,
            TemplateId: null,
            HtmlContent: null,
            ValidFromUtc: null,
            ValidUntilUtc: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCampaignCommand.TrackingPageTitle));
    }

    [Fact]
    public void CreateCampaignCommandValidator_ShouldRejectHtmlContentExceedingMaxLength()
    {
        var validator = new CreateCampaignCommandValidator();
        var command = new CreateCampaignCommand(
            Name: "Campaign",
            TrackingPageSlug: "valid-slug",
            TrackingPageTitle: "Title",
            TrackingPageDescription: null,
            TemplateId: null,
            HtmlContent: new string('x', 200001),
            ValidFromUtc: null,
            ValidUntilUtc: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateCampaignCommand.HtmlContent));
    }

    [Fact]
    public void CreateCampaignCommandValidator_ShouldRejectInvalidDateRange()
    {
        var validator = new CreateCampaignCommandValidator();
        var from = DateTimeOffset.UtcNow.AddDays(5);
        var command = new CreateCampaignCommand(
            Name: "Campaign",
            TrackingPageSlug: "valid-slug",
            TrackingPageTitle: "Title",
            TrackingPageDescription: null,
            TemplateId: null,
            HtmlContent: null,
            ValidFromUtc: from,
            ValidUntilUtc: from.AddDays(-1));

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void UpdateCampaignCommandValidator_ShouldAcceptValidCommand()
    {
        var validator = new UpdateCampaignCommandValidator();
        var command = new UpdateCampaignCommand(
            CampaignId: Guid.NewGuid(),
            Name: "Valid Name",
            StartsAtUtc: null,
            EndsAtUtc: null);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateCampaignCommandValidator_ShouldRejectEmptyCampaignId()
    {
        var validator = new UpdateCampaignCommandValidator();
        var command = new UpdateCampaignCommand(
            CampaignId: Guid.Empty,
            Name: "Name");

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCampaignCommand.CampaignId));
    }

    [Fact]
    public void UpdateCampaignCommandValidator_ShouldRejectEmptyName()
    {
        var validator = new UpdateCampaignCommandValidator();
        var command = new UpdateCampaignCommand(
            CampaignId: Guid.NewGuid(),
            Name: "");

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateCampaignCommand.Name));
    }

    [Fact]
    public void UpdateCampaignCommandValidator_ShouldRejectEndBeforeStart()
    {
        var validator = new UpdateCampaignCommandValidator();
        var startsAt = DateTimeOffset.UtcNow.AddDays(2);
        var command = new UpdateCampaignCommand(
            CampaignId: Guid.NewGuid(),
            Name: "Name",
            StartsAtUtc: startsAt,
            EndsAtUtc: startsAt.AddDays(-1));

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void ScheduleCampaignCommandValidator_ShouldAcceptValidCommand()
    {
        var validator = new ScheduleCampaignCommandValidator();
        var command = new ScheduleCampaignCommand(
            CampaignId: Guid.NewGuid(),
            StartsAtUtc: DateTimeOffset.UtcNow.AddDays(1),
            EndsAtUtc: DateTimeOffset.UtcNow.AddDays(2));

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ScheduleCampaignCommandValidator_ShouldRejectEmptyCampaignId()
    {
        var validator = new ScheduleCampaignCommandValidator();
        var command = new ScheduleCampaignCommand(
            CampaignId: Guid.Empty,
            StartsAtUtc: DateTimeOffset.UtcNow.AddDays(1),
            EndsAtUtc: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ScheduleCampaignCommand.CampaignId));
    }

    [Fact]
    public void ScheduleCampaignCommandValidator_ShouldRejectMinValueStart()
    {
        var validator = new ScheduleCampaignCommandValidator();
        var command = new ScheduleCampaignCommand(
            CampaignId: Guid.NewGuid(),
            StartsAtUtc: DateTimeOffset.MinValue,
            EndsAtUtc: null);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ScheduleCampaignCommand.StartsAtUtc));
    }

    [Fact]
    public void ScheduleCampaignCommandValidator_ShouldRejectEndBeforeStart()
    {
        var validator = new ScheduleCampaignCommandValidator();
        var startsAt = DateTimeOffset.UtcNow.AddDays(2);
        var command = new ScheduleCampaignCommand(
            CampaignId: Guid.NewGuid(),
            StartsAtUtc: startsAt,
            EndsAtUtc: startsAt.AddDays(-1));

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeleteCampaignCommandValidator_ShouldRejectEmptyCampaignId()
    {
        var validator = new DeleteCampaignCommandValidator();
        var command = new DeleteCampaignCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(DeleteCampaignCommand.CampaignId));
    }

    [Fact]
    public void DeleteCampaignCommandValidator_ShouldAcceptValidCommand()
    {
        var validator = new DeleteCampaignCommandValidator();
        var command = new DeleteCampaignCommand(Guid.NewGuid());

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void StartCampaignCommandValidator_ShouldRejectEmptyCampaignId()
    {
        var validator = new StartCampaignCommandValidator();
        var command = new StartCampaignCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(StartCampaignCommand.CampaignId));
    }

    [Fact]
    public void StartCampaignCommandValidator_ShouldAcceptValidCommand()
    {
        var validator = new StartCampaignCommandValidator();
        var command = new StartCampaignCommand(Guid.NewGuid());

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void PauseCampaignCommandValidator_ShouldRejectEmptyCampaignId()
    {
        var validator = new PauseCampaignCommandValidator();
        var command = new PauseCampaignCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(PauseCampaignCommand.CampaignId));
    }

    [Fact]
    public void CompleteCampaignCommandValidator_ShouldRejectEmptyCampaignId()
    {
        var validator = new CompleteCampaignCommandValidator();
        var command = new CompleteCampaignCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CompleteCampaignCommand.CampaignId));
    }

    [Fact]
    public void CancelCampaignCommandValidator_ShouldRejectEmptyCampaignId()
    {
        var validator = new CancelCampaignCommandValidator();
        var command = new CancelCampaignCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CancelCampaignCommand.CampaignId));
    }

    private static TrackingPageAggregate CreateTrackingPage(string slug)
        => new(
            Guid.NewGuid(),
            new TrackingPageSlug(slug),
            $"{slug} title",
            null,
            "owner-1",
            null,
            null,
            null,
            null,
            null);

    private sealed class FakeCampaignRepository : ICampaignRepository
    {
        private readonly CampaignAggregate? _campaign;

        public FakeCampaignRepository()
        {
        }

        public FakeCampaignRepository(CampaignAggregate? campaign)
        {
            _campaign = campaign;
        }

        public CampaignAggregate? SavedCampaign { get; private set; }
        public CampaignAggregate? DeletedAggregate { get; private set; }

        public Task<CampaignAggregate?> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken)
            => Task.FromResult(_campaign?.Id == campaignId ? _campaign : null);

        public Task<CampaignAggregate?> GetByTrackingPageIdAsync(Guid trackingPageId, CancellationToken cancellationToken)
            => Task.FromResult(_campaign?.TrackingPageId == trackingPageId ? _campaign : null);

        public Task<IReadOnlyCollection<CampaignAggregate>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<CampaignAggregate>>(
                _campaign is null ? Array.Empty<CampaignAggregate>() : new[] { _campaign });

        public Task SaveAsync(CampaignAggregate aggregate, CancellationToken cancellationToken)
        {
            SavedCampaign = aggregate;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(CampaignAggregate aggregate, CancellationToken cancellationToken)
        {
            DeletedAggregate = aggregate;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeTrackingPageRepository : ITrackingPageRepository
    {
        private readonly TrackingPageAggregate? _aggregate;

        public FakeTrackingPageRepository()
        {
        }

        public FakeTrackingPageRepository(TrackingPageAggregate? aggregate)
        {
            _aggregate = aggregate;
        }

        public bool SlugExistsResult { get; init; }

        public TrackingPageAggregate? SavedAggregate { get; private set; }
        public TrackingPageAggregate? DeletedAggregate { get; private set; }

        public Task<TrackingPageAggregate?> GetByIdAsync(Guid trackingPageId, CancellationToken cancellationToken)
            => Task.FromResult(_aggregate?.Id == trackingPageId ? _aggregate : null);

        public Task<TrackingPageAggregate?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
            => Task.FromResult<TrackingPageAggregate?>(_aggregate?.Slug.Value == slug ? _aggregate : null);

        public Task<IReadOnlyCollection<TrackingPageAggregate>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<TrackingPageAggregate>>(
                _aggregate is null ? Array.Empty<TrackingPageAggregate>() : new[] { _aggregate });

        public Task<bool> SlugExistsAsync(string slug, Guid? excludingTrackingPageId, CancellationToken cancellationToken)
            => Task.FromResult(SlugExistsResult);

        public Task SaveAsync(TrackingPageAggregate aggregate, CancellationToken cancellationToken)
        {
            SavedAggregate = aggregate;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TrackingPageAggregate aggregate, CancellationToken cancellationToken)
        {
            DeletedAggregate = aggregate;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public string? UserId => "test-user";
        public bool IsAuthenticated => true;
        public IReadOnlyCollection<string> Roles => new[] { "Admin" };
    }
}
