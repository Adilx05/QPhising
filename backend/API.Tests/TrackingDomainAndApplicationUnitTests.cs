using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.CQRS.Commands.Tracking;
using QPhising.Application.CQRS.Queries.Tracking;
using QPhising.Application.Mapping.Tracking;
using QPhising.Domain.Tracking.Aggregates;
using QPhising.Domain.Tracking.Entities;
using QPhising.Domain.Tracking.Enums;
using QPhising.Domain.Tracking.Models;
using QPhising.Domain.Tracking.ValueObjects;
using Xunit;

namespace QPhising.Api.Tests;

public sealed class TrackingDomainAndApplicationUnitTests
{
    [Fact]
    public void TrackingPageSlug_ShouldNormalizeAndRejectInvalidPattern()
    {
        var slug = new TrackingPageSlug("  Enterprise-Launch-2026  ");

        Assert.Equal("enterprise-launch-2026", slug.Value);
        Assert.Throws<ArgumentException>(() => new TrackingPageSlug("invalid slug"));
    }

    [Fact]
    public void TrackingPageAggregate_ShouldRejectMutationAfterArchive()
    {
        var aggregate = new TrackingPageAggregate(
            Guid.NewGuid(),
            new TrackingPageSlug("immutable-page"),
            "Immutable Page",
            "description",
            new TrackingPageUrl("https://example.com/page"),
            "owner-1",
            new PageSettings(30, maskIpAddress: true, enableBotFiltering: true, captureUtmParameters: true));

        aggregate.Archive();

        Assert.Throws<InvalidOperationException>(() => aggregate.UpdateDetails(
            new TrackingPageSlug("new-slug"),
            "Updated",
            "updated",
            new TrackingPageUrl("https://example.com/new")));
    }

    [Fact]
    public void VisitEventEntity_ShouldRequireIpHashWhenPolicyEnabled()
    {
        Assert.Throws<ArgumentException>(() => new VisitEventEntity(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            new TrackingIdentifier("session-1"),
            new TrackingIdentifier("visitor-1"),
            "Mozilla/5.0",
            "https://ref.example",
            null,
            IpAddressHashPolicy.Sha256));
    }

    [Fact]
    public async Task IngestVisitEventCommandHandler_ShouldReturnDuplicateWithoutPersisting()
    {
        var trackingPage = new TrackingPageAggregate(
            Guid.NewGuid(),
            new TrackingPageSlug("dedupe-page"),
            "Dedupe Page",
            null,
            new TrackingPageUrl("https://example.com/dedupe"),
            "owner-1");

        var trackingRepo = new FakeTrackingPageRepository(trackingPage);
        var visitRepo = new FakeVisitEventRepository
        {
            DuplicateResult = true
        };

        var handler = new IngestVisitEventCommandHandler(trackingRepo, visitRepo);
        var result = await handler.Handle(
            new IngestVisitEventCommand(
                trackingPage.Id,
                DateTimeOffset.UtcNow,
                "session-1",
                "visitor-1",
                "Mozilla/5.0",
                "https://ref.example",
                "hashed-ip",
                IpAddressHashPolicy.Sha256,
                DeduplicationWindowSeconds: 180),
            CancellationToken.None);

        Assert.True(result.IsDuplicate);
        Assert.Equal(Guid.Empty, result.VisitEventId);
        Assert.Empty(visitRepo.SavedEvents);
    }

    [Fact]
    public void CreateTrackingPageCommandValidator_ShouldRejectOutOfRangeRetention()
    {
        var validator = new CreateTrackingPageCommandValidator();
        var command = new CreateTrackingPageCommand(
            Slug: "valid-slug",
            Title: "Valid title",
            Description: null,
            DestinationUrl: "https://example.com",
            OwnerId: "owner-1",
            RetentionDays: 10000,
            MaskIpAddress: true,
            EnableBotFiltering: true,
            CaptureUtmParameters: true);

        var result = validator.Validate(command);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateTrackingPageCommand.RetentionDays));
    }

    [Fact]
    public async Task GetTrackingAnalyticsOverviewQueryHandler_ShouldMapRepositoryMetricsToResponse()
    {
        var now = DateTimeOffset.UtcNow;
        var visitRepo = new FakeVisitEventRepository
        {
            TotalAcrossPages = 42,
            UniqueAcrossPages = 27,
            TopPages =
            [
                new TrackingTopPageMetric(Guid.NewGuid(), "landing-page", "Landing", 30, 20)
            ],
            RecentAcrossPages =
            [
                new TrackingRecentVisitMetric(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    "landing-page",
                    now,
                    "session-1",
                    "fingerprint-1",
                    "Mozilla/5.0",
                    "https://ref.example",
                    "hashed-ip",
                    IpAddressHashPolicy.Sha256)
            ],
            TrendAcrossPages =
            [
                new TrackingVisitTrendBucket(now.AddHours(-1), 10, 8)
            ]
        };

        var handler = new GetTrackingAnalyticsOverviewQueryHandler(visitRepo);
        var response = await handler.Handle(
            new GetTrackingAnalyticsOverviewQuery(
                now.AddDays(-1),
                now,
                TrackingVisitTrendBucketWindow.Day,
                timezoneOffsetMinutes: 0,
                excludeBots: true,
                topPagesLimit: 5,
                recentVisitLimit: 10),
            CancellationToken.None);

        Assert.Equal(42, response.Summary.TotalVisits);
        Assert.Equal(27, response.Summary.UniqueVisitors);
        Assert.Single(response.TopPages);
        Assert.Single(response.RecentVisits);
        Assert.Single(response.Trends);
        Assert.NotEmpty(response.MetricDefinitions);
    }

    [Fact]
    public void TrackingMappingProfile_ToResult_ShouldMapSettings()
    {
        var aggregate = new TrackingPageAggregate(
            Guid.NewGuid(),
            new TrackingPageSlug("profile-page"),
            "Profile Page",
            "Description",
            new TrackingPageUrl("https://example.com/profile"),
            "owner-2",
            new PageSettings(90, maskIpAddress: true, enableBotFiltering: false, captureUtmParameters: true));

        var result = aggregate.ToResult();

        Assert.Equal(aggregate.Slug.Value, result.Slug);
        Assert.NotNull(result.Settings);
        Assert.True(result.Settings!.MaskIpAddress);
        Assert.Equal(90, result.Settings.RetentionDays);
    }

    private sealed class FakeTrackingPageRepository : ITrackingPageRepository
    {
        private readonly TrackingPageAggregate _aggregate;

        public FakeTrackingPageRepository(TrackingPageAggregate aggregate)
        {
            _aggregate = aggregate;
        }

        public Task<TrackingPageAggregate?> GetByIdAsync(Guid trackingPageId, CancellationToken cancellationToken)
            => Task.FromResult<TrackingPageAggregate?>(_aggregate.Id == trackingPageId ? _aggregate : null);

        public Task<TrackingPageAggregate?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
            => Task.FromResult<TrackingPageAggregate?>(_aggregate.Slug.Value == slug ? _aggregate : null);

        public Task<IReadOnlyCollection<TrackingPageAggregate>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<TrackingPageAggregate>>([_aggregate]);

        public Task<bool> SlugExistsAsync(string slug, Guid? excludingTrackingPageId, CancellationToken cancellationToken)
            => Task.FromResult(false);

        public Task SaveAsync(TrackingPageAggregate aggregate, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task DeleteAsync(TrackingPageAggregate aggregate, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class FakeVisitEventRepository : IVisitEventRepository
    {
        public bool DuplicateResult { get; init; }

        public List<VisitEventEntity> SavedEvents { get; } = [];

        public int TotalAcrossPages { get; init; }

        public int UniqueAcrossPages { get; init; }

        public IReadOnlyCollection<TrackingTopPageMetric> TopPages { get; init; } = Array.Empty<TrackingTopPageMetric>();

        public IReadOnlyCollection<TrackingRecentVisitMetric> RecentAcrossPages { get; init; } = Array.Empty<TrackingRecentVisitMetric>();

        public IReadOnlyCollection<TrackingVisitTrendBucket> TrendAcrossPages { get; init; } = Array.Empty<TrackingVisitTrendBucket>();

        public Task<bool> ExistsDuplicateAsync(Guid trackingPageId, string sessionId, string visitorFingerprint, DateTimeOffset occurredAtUtc, TimeSpan deduplicationWindow, CancellationToken cancellationToken)
            => Task.FromResult(DuplicateResult);

        public Task SaveAsync(VisitEventEntity visitEvent, CancellationToken cancellationToken)
        {
            SavedEvents.Add(visitEvent);
            return Task.CompletedTask;
        }

        public Task<int> CountTotalAsync(Guid trackingPageId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken)
            => Task.FromResult(0);

        public Task<int> CountUniqueVisitorsAsync(Guid trackingPageId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, CancellationToken cancellationToken)
            => Task.FromResult(0);

        public Task<DateTimeOffset?> GetLastVisitAtUtcAsync(Guid trackingPageId, CancellationToken cancellationToken)
            => Task.FromResult<DateTimeOffset?>(null);

        public Task<IReadOnlyCollection<TrackingVisitTrendBucket>> GetTrendBucketsAsync(Guid trackingPageId, DateTimeOffset fromUtc, DateTimeOffset toUtc, int bucketSizeMinutes, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<TrackingVisitTrendBucket>>(Array.Empty<TrackingVisitTrendBucket>());

        public Task<IReadOnlyCollection<VisitEventEntity>> ListRecentAsync(Guid trackingPageId, DateTimeOffset? fromUtc, DateTimeOffset? toUtc, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<VisitEventEntity>>(Array.Empty<VisitEventEntity>());

        public Task<int> CountTotalAcrossPagesAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, bool excludeBots, CancellationToken cancellationToken)
            => Task.FromResult(TotalAcrossPages);

        public Task<int> CountUniqueVisitorsAcrossPagesAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, bool excludeBots, CancellationToken cancellationToken)
            => Task.FromResult(UniqueAcrossPages);

        public Task<IReadOnlyCollection<TrackingTopPageMetric>> ListTopPagesAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, bool excludeBots, int limit, CancellationToken cancellationToken)
            => Task.FromResult(TopPages);

        public Task<IReadOnlyCollection<TrackingRecentVisitMetric>> ListRecentAcrossPagesAsync(DateTimeOffset? fromUtc, DateTimeOffset? toUtc, bool excludeBots, int limit, CancellationToken cancellationToken)
            => Task.FromResult(RecentAcrossPages);

        public Task<IReadOnlyCollection<TrackingVisitTrendBucket>> GetTrendBucketsAcrossPagesAsync(DateTimeOffset fromUtc, DateTimeOffset toUtc, TrackingVisitTrendBucketWindow window, int timezoneOffsetMinutes, bool excludeBots, CancellationToken cancellationToken)
            => Task.FromResult(TrendAcrossPages);
    }
}
