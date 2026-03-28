using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Campaigns;
using QPhising.Infrastructure.Exports;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class PdfExportPipelineTests
{
    [Fact]
    public async Task BuildCampaignReportAsync_Should_CreatePdfBinaryWithExpectedMetadata()
    {
        QuestPdfExportService service = new();

        Campaign draftCampaign = Campaign.Create(
            "Quarterly Simulation",
            TemplateType.Invoice,
            "<html>simulation</html>",
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(8));

        Campaign scheduledCampaign = Campaign.Create(
            "HR Credential Drill",
            TemplateType.HRNotice,
            "<html>hr</html>",
            DateTimeOffset.UtcNow.AddDays(4),
            DateTimeOffset.UtcNow.AddDays(12));
        scheduledCampaign.Schedule(DateTimeOffset.UtcNow);

        var result = await service.BuildCampaignReportAsync([draftCampaign, scheduledCampaign], CancellationToken.None);

        Assert.Equal("application/pdf", result.ContentType);
        Assert.StartsWith("campaign-report-", result.FileName, StringComparison.Ordinal);
        Assert.EndsWith(".pdf", result.FileName, StringComparison.Ordinal);
        Assert.NotEmpty(result.Content);
        Assert.True(result.Content.Length > 500, "Generated campaign PDF should contain report structure.");
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(result.Content.AsSpan(0, 5)));
    }

    [Fact]
    public async Task BuildAnalyticsReportAsync_Should_CreatePdfBinaryWithExpectedMetadata()
    {
        QuestPdfExportService service = new();

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
                new TemplateTypeBreakdownReadModel(TemplateType.Invoice, 2, 90, 10),
                new TemplateTypeBreakdownReadModel(TemplateType.HRNotice, 1, 50, 6)
            ]);

        AnalyticsReadCriteria criteria = new(
            From: from,
            To: to,
            CampaignIds: Array.Empty<Guid>(),
            TemplateTypes: Array.Empty<TemplateType>(),
            CampaignStatuses: Array.Empty<CampaignStatus>());

        var result = await service.BuildAnalyticsReportAsync(dashboard, criteria, CancellationToken.None);

        Assert.Equal("application/pdf", result.ContentType);
        Assert.StartsWith("analytics-report-", result.FileName, StringComparison.Ordinal);
        Assert.EndsWith(".pdf", result.FileName, StringComparison.Ordinal);
        Assert.NotEmpty(result.Content);
        Assert.True(result.Content.Length > 500, "Generated analytics PDF should contain report structure.");
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(result.Content.AsSpan(0, 5)));
    }
}
