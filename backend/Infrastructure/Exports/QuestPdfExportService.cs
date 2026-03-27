using QPhising.Application.Common.Abstractions;
using QPhising.Application.Common.Abstractions.Exports;
using QPhising.Domain.Campaigns;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QPhising.Infrastructure.Exports;

public sealed class QuestPdfExportService : IPdfExportService
{
    private const string PdfContentType = "application/pdf";

    static QuestPdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<ExportBinaryFile> BuildCampaignReportAsync(
        IReadOnlyCollection<Campaign> campaigns,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Campaign[] orderedCampaigns = campaigns
            .OrderBy(campaign => campaign.StartDate)
            .ThenBy(campaign => campaign.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        byte[] content = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(32);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(style => style.FontSize(10));

                    page.Header()
                        .Text("Campaign Report")
                        .SemiBold()
                        .FontSize(20)
                        .FontColor(Colors.Blue.Medium);

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);
                        column.Item().Text($"Generated (UTC): {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}");
                        column.Item().Text($"Total Campaigns: {orderedCampaigns.Length}");

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(StyleHeaderCell).Text("Metric");
                                header.Cell().Element(StyleHeaderCell).Text("Value");
                            });

                            AddMetricRow(table, "Draft", orderedCampaigns.Count(c => c.Status == CampaignStatus.Draft).ToString());
                            AddMetricRow(table, "Scheduled", orderedCampaigns.Count(c => c.Status == CampaignStatus.Scheduled).ToString());
                            AddMetricRow(table, "Active", orderedCampaigns.Count(c => c.Status == CampaignStatus.Active).ToString());
                            AddMetricRow(table, "Ended", orderedCampaigns.Count(c => c.Status == CampaignStatus.Ended).ToString());
                            AddMetricRow(table, "Archived", orderedCampaigns.Count(c => c.Status == CampaignStatus.Archived).ToString());
                        });

                        column.Item().Text("Campaign Details").SemiBold().FontSize(14);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(StyleHeaderCell).Text("Name");
                                header.Cell().Element(StyleHeaderCell).Text("Template");
                                header.Cell().Element(StyleHeaderCell).Text("Status");
                                header.Cell().Element(StyleHeaderCell).Text("Start (UTC)");
                                header.Cell().Element(StyleHeaderCell).Text("End (UTC)");
                            });

                            foreach (Campaign campaign in orderedCampaigns)
                            {
                                AddDataCell(table, campaign.Name);
                                AddDataCell(table, campaign.TemplateType.ToString());
                                AddDataCell(table, campaign.Status.ToString());
                                AddDataCell(table, campaign.StartDate.ToString("yyyy-MM-dd HH:mm"));
                                AddDataCell(table, campaign.EndDate.ToString("yyyy-MM-dd HH:mm"));
                            }
                        });
                    });
                });
            })
            .GeneratePdf();

        return Task.FromResult(new ExportBinaryFile(
            $"campaign-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf",
            PdfContentType,
            content));
    }

    public Task<ExportBinaryFile> BuildAnalyticsReportAsync(
        AnalyticsDashboardReadModel dashboard,
        AnalyticsReadCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        byte[] content = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(32);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(style => style.FontSize(10));

                    page.Header()
                        .Text("Analytics KPI Report")
                        .SemiBold()
                        .FontSize(20)
                        .FontColor(Colors.Blue.Medium);

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);
                        column.Item().Text($"Window (UTC): {criteria.From:yyyy-MM-dd HH:mm} -> {criteria.To:yyyy-MM-dd HH:mm}");

                        column.Item().Text("KPI Summary").SemiBold().FontSize(14);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(StyleHeaderCell).Text("KPI");
                                header.Cell().Element(StyleHeaderCell).Text("Value");
                            });

                            AddMetricRow(table, "Campaign Total", dashboard.CampaignTotal.ToString());
                            AddMetricRow(table, "Campaign Active", dashboard.CampaignActive.ToString());
                            AddMetricRow(table, "Clicks Total", dashboard.ClickTotal.ToString());
                            AddMetricRow(table, "Clicks Unique", dashboard.ClickUnique.ToString());
                            AddMetricRow(table, "Conversions", dashboard.ConversionTotal.ToString());
                            AddMetricRow(table, "Tasks Enqueued", dashboard.TasksEnqueued.ToString());
                            AddMetricRow(table, "Tasks Processed", dashboard.TasksProcessed.ToString());
                            AddMetricRow(table, "Tasks Failed", dashboard.TasksFailed.ToString());
                            AddMetricRow(table, "Task Avg Duration (ms)", dashboard.AverageTaskDurationMilliseconds.ToString("0.##"));
                        });

                        column.Item().Text("Trend (UTC Buckets)").SemiBold().FontSize(14);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(StyleHeaderCell).Text("Bucket");
                                header.Cell().Element(StyleHeaderCell).Text("Clicks");
                                header.Cell().Element(StyleHeaderCell).Text("Conversions");
                                header.Cell().Element(StyleHeaderCell).Text("Tasks");
                            });

                            foreach (AnalyticsTrendReadModel trend in dashboard.Trend.OrderBy(item => item.BucketStartUtc))
                            {
                                AddDataCell(table, trend.BucketStartUtc.ToString("yyyy-MM-dd HH:mm"));
                                AddDataCell(table, trend.Clicks.ToString());
                                AddDataCell(table, trend.Conversions.ToString());
                                AddDataCell(table, trend.TasksProcessed.ToString());
                            }
                        });
                    });
                });
            })
            .GeneratePdf();

        return Task.FromResult(new ExportBinaryFile(
            $"analytics-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.pdf",
            PdfContentType,
            content));
    }

    private static IContainer StyleHeaderCell(IContainer container)
    {
        return container
            .Background(Colors.Blue.Darken2)
            .PaddingVertical(6)
            .PaddingHorizontal(4)
            .DefaultTextStyle(style => style.FontColor(Colors.White).SemiBold());
    }

    private static void AddMetricRow(TableDescriptor table, string metric, string value)
    {
        AddDataCell(table, metric);
        AddDataCell(table, value);
    }

    private static void AddDataCell(TableDescriptor table, string value)
    {
        table.Cell()
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(4)
            .PaddingHorizontal(4)
            .Text(value);
    }
}
