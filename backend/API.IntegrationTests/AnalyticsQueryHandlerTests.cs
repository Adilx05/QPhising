using QPhising.Application.Common.Abstractions;
using QPhising.Application.Features.Analytics.GetDashboardKpis;
using QPhising.Domain.Campaigns;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class AnalyticsQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Map_Read_Model_To_Kpi_Response_With_Derived_Rates()
    {
        var from = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(1);

        AnalyticsDashboardReadModel readModel = new(
            CampaignTotal: 4,
            CampaignDraft: 1,
            CampaignScheduled: 1,
            CampaignActive: 1,
            CampaignEnded: 1,
            CampaignArchived: 0,
            ClickTotal: 120,
            ClickUnique: 80,
            ConversionTotal: 20,
            TasksEnqueued: 50,
            TasksProcessed: 40,
            TasksSucceeded: 30,
            TasksFailed: 10,
            TasksRetried: 6,
            TasksDeadLettered: 2,
            AverageTaskDurationMilliseconds: 132.75m,
            Trend:
            [
                new AnalyticsTrendReadModel(from, 12, 4, 7),
                new AnalyticsTrendReadModel(from.AddHours(1), 8, 2, 5)
            ],
            CampaignStatusBreakdown:
            [
                new CampaignStatusBreakdownReadModel(CampaignStatus.Draft, 1, 5, 0),
                new CampaignStatusBreakdownReadModel(CampaignStatus.Active, 1, 50, 20)
            ],
            TemplateTypeBreakdown:
            [
                new TemplateTypeBreakdownReadModel(TemplateType.Email, 3, 90, 18),
                new TemplateTypeBreakdownReadModel(TemplateType.LandingPage, 1, 30, 2)
            ]);

        var repository = new StubAnalyticsReadRepository(readModel);
        var cache = new InMemoryAnalyticsDashboardCache();
        var handler = new GetDashboardKpisQueryHandler(repository, cache);

        var result = await handler.Handle(new GetDashboardKpisQuery(
            from,
            to,
            AnalyticsTimeGrain.Day,
            "UTC"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(4, result.Value!.Campaigns.Total);
        Assert.Equal(100m, result.Value.Clicks.ClickThroughRatePercent);
        Assert.Equal(25m, result.Value.Conversions.ConversionRatePercent);
        Assert.Equal(75m, result.Value.TaskThroughput.SuccessRatePercent);
        Assert.Equal(30m, result.Value.Trend.First().ConversionRatePercent);
        Assert.Equal(20m, result.Value.TemplateTypeBreakdown.First().ConversionRatePercent);
        Assert.Equal(1, repository.CallCount);
        Assert.Equal(1, cache.SetCount);
    }

    [Fact]
    public async Task Handle_Should_Return_Cached_Response_Without_Hitting_Repository()
    {
        var from = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var to = from.AddDays(1);
        var query = new GetDashboardKpisQuery(from, to, AnalyticsTimeGrain.Day, "UTC");

        var repository = new StubAnalyticsReadRepository(new AnalyticsDashboardReadModel(
            CampaignTotal: 1,
            CampaignDraft: 1,
            CampaignScheduled: 0,
            CampaignActive: 0,
            CampaignEnded: 0,
            CampaignArchived: 0,
            ClickTotal: 1,
            ClickUnique: 1,
            ConversionTotal: 1,
            TasksEnqueued: 1,
            TasksProcessed: 1,
            TasksSucceeded: 1,
            TasksFailed: 0,
            TasksRetried: 0,
            TasksDeadLettered: 0,
            AverageTaskDurationMilliseconds: 1m,
            Trend: [],
            CampaignStatusBreakdown: [],
            TemplateTypeBreakdown: []));
        var cache = new InMemoryAnalyticsDashboardCache();
        var cachedResponse = new DashboardKpisResponse(
            new AnalyticsFilterDimensions(from, to, AnalyticsTimeGrain.Day, "UTC", [], [], []),
            new CampaignKpiSummary(9, 1, 2, 3, 2, 1),
            new ClickKpiSummary(100, 90, 10m, 0m),
            new ConversionKpiSummary(20, 20m, 0m),
            new TaskThroughputKpiSummary(10, 10, 10, 0, 0, 0, 100m, 10m),
            [],
            [],
            []);

        await cache.SetAsync(query, cachedResponse, CancellationToken.None);
        var handler = new GetDashboardKpisQueryHandler(repository, cache);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(9, result.Value!.Campaigns.Total);
        Assert.Equal(0, repository.CallCount);
    }

    private sealed class StubAnalyticsReadRepository(AnalyticsDashboardReadModel readModel) : IAnalyticsReadRepository
    {
        public int CallCount { get; private set; }

        public Task<AnalyticsDashboardReadModel> GetDashboardReadModelAsync(AnalyticsReadCriteria criteria, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(readModel);
        }
    }

    private sealed class InMemoryAnalyticsDashboardCache : IAnalyticsDashboardCache
    {
        private readonly Dictionary<string, DashboardKpisResponse> _store = new(StringComparer.Ordinal);

        public int SetCount { get; private set; }

        public Task<DashboardKpisResponse?> GetAsync(GetDashboardKpisQuery query, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(Key(query), out DashboardKpisResponse? response);
            return Task.FromResult(response);
        }

        public Task SetAsync(GetDashboardKpisQuery query, DashboardKpisResponse response, CancellationToken cancellationToken = default)
        {
            _store[Key(query)] = response;
            SetCount++;
            return Task.CompletedTask;
        }

        public Task InvalidateAsync(CancellationToken cancellationToken = default)
        {
            _store.Clear();
            return Task.CompletedTask;
        }

        private static string Key(GetDashboardKpisQuery query)
        {
            return string.Join('|', query.From.ToUnixTimeSeconds(), query.To.ToUnixTimeSeconds(), query.TimeGrain, query.TimeZone);
        }
    }
}
