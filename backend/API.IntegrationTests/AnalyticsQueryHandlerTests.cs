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
        var handler = new GetDashboardKpisQueryHandler(repository);

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
    }

    private sealed class StubAnalyticsReadRepository(AnalyticsDashboardReadModel readModel) : IAnalyticsReadRepository
    {
        public Task<AnalyticsDashboardReadModel> GetDashboardReadModelAsync(AnalyticsReadCriteria criteria, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(readModel);
        }
    }
}
