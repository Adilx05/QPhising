using ClosedXML.Excel;
using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Campaigns;
using QPhising.Infrastructure.Exports;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class ExcelExportPipelineTests
{
    [Fact]
    public async Task BuildCampaignReportAsync_Should_CreateWorkbookWithSummaryAndCampaignSheets()
    {
        ClosedXmlExcelExportService service = new();

        Campaign draftCampaign = Campaign.Create(
            "Quarterly Simulation",
            TemplateType.Email,
            "<html>simulation</html>",
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(8));

        Campaign scheduledCampaign = Campaign.Create(
            "HR Credential Drill",
            TemplateType.LandingPage,
            "<html>hr</html>",
            DateTimeOffset.UtcNow.AddDays(4),
            DateTimeOffset.UtcNow.AddDays(12));
        scheduledCampaign.Schedule(DateTimeOffset.UtcNow);

        var result = await service.BuildCampaignReportAsync([draftCampaign, scheduledCampaign], CancellationToken.None);

        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
        Assert.StartsWith("campaign-report-", result.FileName, StringComparison.Ordinal);
        Assert.NotEmpty(result.Content);

        using var stream = new MemoryStream(result.Content);
        using var workbook = new XLWorkbook(stream);

        IXLWorksheet summarySheet = workbook.Worksheet("Summary");
        IXLWorksheet campaignsSheet = workbook.Worksheet("Campaigns");

        Assert.Equal("Campaign Report", summarySheet.Cell(1, 1).GetString());
        Assert.Equal("Metric", summarySheet.Cell(5, 1).GetString());
        Assert.Equal("Total campaigns", summarySheet.Cell(6, 1).GetString());
        Assert.Equal(2, summarySheet.Cell(6, 2).GetValue<int>());

        Assert.Equal("CampaignId", campaignsSheet.Cell(1, 1).GetString());
        Assert.Equal("Name", campaignsSheet.Cell(1, 2).GetString());
        Assert.Equal("Status", campaignsSheet.Cell(1, 4).GetString());
        Assert.Equal(2, campaignsSheet.RowsUsed().Count() - 1);
    }

    [Fact]
    public async Task BuildAnalyticsReportAsync_Should_CreateWorkbookWithKpiAndBreakdownSheets()
    {
        ClosedXmlExcelExportService service = new();

        DateTimeOffset from = new(2026, 3, 20, 0, 0, 0, TimeSpan.Zero);
        DateTimeOffset to = new(2026, 3, 20, 2, 0, 0, TimeSpan.Zero);

        AnalyticsDashboardReadModel dashboard = new(
            CampaignTotal: 3,
            CampaignDraft: 1,
            CampaignScheduled: 1,
            CampaignActive: 1,
            CampaignEnded: 0,
            CampaignArchived: 0,
            ClickTotal: 140,
            ClickUnique: 80,
            ConversionTotal: 16,
            TasksEnqueued: 170,
            TasksProcessed: 160,
            TasksSucceeded: 150,
            TasksFailed: 10,
            TasksRetried: 4,
            TasksDeadLettered: 1,
            AverageTaskDurationMilliseconds: 122.50m,
            Trend:
            [
                new AnalyticsTrendReadModel(from, 40, 5, 50),
                new AnalyticsTrendReadModel(from.AddHours(1), 45, 5, 52),
                new AnalyticsTrendReadModel(from.AddHours(2), 55, 6, 58)
            ],
            CampaignStatusBreakdown:
            [
                new CampaignStatusBreakdownReadModel(CampaignStatus.Active, 1, 70, 8),
                new CampaignStatusBreakdownReadModel(CampaignStatus.Scheduled, 1, 50, 5),
                new CampaignStatusBreakdownReadModel(CampaignStatus.Draft, 1, 20, 3)
            ],
            TemplateTypeBreakdown:
            [
                new TemplateTypeBreakdownReadModel(TemplateType.Email, 2, 90, 10),
                new TemplateTypeBreakdownReadModel(TemplateType.LandingPage, 1, 50, 6)
            ]);

        AnalyticsReadCriteria criteria = new(
            From: from,
            To: to,
            CampaignIds: Array.Empty<Guid>(),
            TemplateTypes: Array.Empty<TemplateType>(),
            CampaignStatuses: Array.Empty<CampaignStatus>());

        var result = await service.BuildAnalyticsReportAsync(dashboard, criteria, CancellationToken.None);

        Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", result.ContentType);
        Assert.StartsWith("analytics-report-", result.FileName, StringComparison.Ordinal);

        using var stream = new MemoryStream(result.Content);
        using var workbook = new XLWorkbook(stream);

        IXLWorksheet kpiSheet = workbook.Worksheet("KPIs");
        IXLWorksheet trendsSheet = workbook.Worksheet("Trends");
        IXLWorksheet campaignStatusSheet = workbook.Worksheet("CampaignStatus");
        IXLWorksheet templateTypeSheet = workbook.Worksheet("TemplateType");

        Assert.Equal("Analytics KPI Report", kpiSheet.Cell(1, 1).GetString());
        Assert.Equal("Campaign Total", kpiSheet.Cell(6, 1).GetString());
        Assert.Equal(3, kpiSheet.Cell(6, 2).GetValue<int>());

        Assert.Equal("BucketStartUtc", trendsSheet.Cell(1, 1).GetString());
        Assert.Equal(3, trendsSheet.RowsUsed().Count() - 1);

        Assert.Equal("Status", campaignStatusSheet.Cell(1, 1).GetString());
        Assert.Equal(3, campaignStatusSheet.RowsUsed().Count() - 1);

        Assert.Equal("TemplateType", templateTypeSheet.Cell(1, 1).GetString());
        Assert.Equal(2, templateTypeSheet.RowsUsed().Count() - 1);
    }
}
