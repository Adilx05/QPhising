using ClosedXML.Excel;
using QPhising.Application.Common.Abstractions;
using QPhising.Application.Common.Abstractions.Exports;
using QPhising.Domain.Campaigns;

namespace QPhising.Infrastructure.Exports;

public sealed class ClosedXmlExcelExportService : IExcelExportService
{
    private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public Task<ExportBinaryFile> BuildCampaignReportAsync(
        IReadOnlyCollection<Campaign> campaigns,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook();
        IXLWorksheet summarySheet = workbook.Worksheets.Add("Summary");
        IXLWorksheet campaignsSheet = workbook.Worksheets.Add("Campaigns");

        int draft = campaigns.Count(c => c.Status == CampaignStatus.Draft);
        int scheduled = campaigns.Count(c => c.Status == CampaignStatus.Scheduled);
        int active = campaigns.Count(c => c.Status == CampaignStatus.Active);
        int ended = campaigns.Count(c => c.Status == CampaignStatus.Ended);
        int archived = campaigns.Count(c => c.Status == CampaignStatus.Archived);

        BuildReportTitle(summarySheet, "Campaign Report");
        summarySheet.Cell(3, 1).Value = "Generated (UTC)";
        summarySheet.Cell(3, 2).Value = DateTimeOffset.UtcNow.UtcDateTime;
        summarySheet.Cell(3, 2).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";

        summarySheet.Cell(5, 1).Value = "Metric";
        summarySheet.Cell(5, 2).Value = "Value";
        StyleHeader(summarySheet.Range(5, 1, 5, 2));

        (string Label, int Value)[] summaryRows =
        [
            ("Total campaigns", campaigns.Count),
            ("Draft", draft),
            ("Scheduled", scheduled),
            ("Active", active),
            ("Ended", ended),
            ("Archived", archived)
        ];

        int currentRow = 6;
        foreach ((string label, int value) in summaryRows)
        {
            summarySheet.Cell(currentRow, 1).Value = label;
            summarySheet.Cell(currentRow, 2).Value = value;
            currentRow++;
        }

        BuildCampaignsTable(campaignsSheet, campaigns.OrderBy(c => c.StartDate).ThenBy(c => c.Name).ToArray());

        summarySheet.Columns().AdjustToContents();
        campaignsSheet.Columns().AdjustToContents();

        return Task.FromResult(ToExportFile(workbook, $"campaign-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.xlsx"));
    }

    public Task<ExportBinaryFile> BuildAnalyticsReportAsync(
        AnalyticsDashboardReadModel dashboard,
        AnalyticsReadCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var workbook = new XLWorkbook();
        IXLWorksheet kpiSheet = workbook.Worksheets.Add("KPIs");
        IXLWorksheet trendsSheet = workbook.Worksheets.Add("Trends");
        IXLWorksheet campaignBreakdownSheet = workbook.Worksheets.Add("CampaignStatus");
        IXLWorksheet templateBreakdownSheet = workbook.Worksheets.Add("TemplateType");

        BuildReportTitle(kpiSheet, "Analytics KPI Report");
        kpiSheet.Cell(3, 1).Value = "Window (UTC)";
        kpiSheet.Cell(3, 2).Value = $"{criteria.From:yyyy-MM-dd HH:mm} -> {criteria.To:yyyy-MM-dd HH:mm}";

        kpiSheet.Cell(5, 1).Value = "KPI";
        kpiSheet.Cell(5, 2).Value = "Value";
        StyleHeader(kpiSheet.Range(5, 1, 5, 2));

        (string Label, object Value)[] kpiRows =
        [
            ("Campaign Total", dashboard.CampaignTotal),
            ("Campaign Active", dashboard.CampaignActive),
            ("Clicks Total", dashboard.ClickTotal),
            ("Clicks Unique", dashboard.ClickUnique),
            ("Conversions", dashboard.ConversionTotal),
            ("Tasks Enqueued", dashboard.TasksEnqueued),
            ("Tasks Processed", dashboard.TasksProcessed),
            ("Tasks Failed", dashboard.TasksFailed),
            ("Task Avg Duration (ms)", dashboard.AverageTaskDurationMilliseconds)
        ];

        int kpiRow = 6;
        foreach ((string label, object value) in kpiRows)
        {
            kpiSheet.Cell(kpiRow, 1).Value = label;
            SetCellValue(kpiSheet.Cell(kpiRow, 2), value);
            kpiRow++;
        }

        trendsSheet.Cell(1, 1).Value = "BucketStartUtc";
        trendsSheet.Cell(1, 2).Value = "Clicks";
        trendsSheet.Cell(1, 3).Value = "Conversions";
        trendsSheet.Cell(1, 4).Value = "TasksProcessed";
        StyleHeader(trendsSheet.Range(1, 1, 1, 4));

        int trendRow = 2;
        foreach (AnalyticsTrendReadModel trend in dashboard.Trend.OrderBy(t => t.BucketStartUtc))
        {
            trendsSheet.Cell(trendRow, 1).Value = trend.BucketStartUtc.UtcDateTime;
            trendsSheet.Cell(trendRow, 1).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            trendsSheet.Cell(trendRow, 2).Value = trend.Clicks;
            trendsSheet.Cell(trendRow, 3).Value = trend.Conversions;
            trendsSheet.Cell(trendRow, 4).Value = trend.TasksProcessed;
            trendRow++;
        }

        campaignBreakdownSheet.Cell(1, 1).Value = "Status";
        campaignBreakdownSheet.Cell(1, 2).Value = "CampaignCount";
        campaignBreakdownSheet.Cell(1, 3).Value = "ClickCount";
        campaignBreakdownSheet.Cell(1, 4).Value = "ConversionCount";
        StyleHeader(campaignBreakdownSheet.Range(1, 1, 1, 4));

        int campaignStatusRow = 2;
        foreach (CampaignStatusBreakdownReadModel row in dashboard.CampaignStatusBreakdown.OrderBy(row => row.Status))
        {
            campaignBreakdownSheet.Cell(campaignStatusRow, 1).Value = row.Status.ToString();
            campaignBreakdownSheet.Cell(campaignStatusRow, 2).Value = row.CampaignCount;
            campaignBreakdownSheet.Cell(campaignStatusRow, 3).Value = row.ClickCount;
            campaignBreakdownSheet.Cell(campaignStatusRow, 4).Value = row.ConversionCount;
            campaignStatusRow++;
        }

        templateBreakdownSheet.Cell(1, 1).Value = "TemplateType";
        templateBreakdownSheet.Cell(1, 2).Value = "CampaignCount";
        templateBreakdownSheet.Cell(1, 3).Value = "ClickCount";
        templateBreakdownSheet.Cell(1, 4).Value = "ConversionCount";
        StyleHeader(templateBreakdownSheet.Range(1, 1, 1, 4));

        int templateRow = 2;
        foreach (TemplateTypeBreakdownReadModel row in dashboard.TemplateTypeBreakdown.OrderBy(row => row.TemplateType))
        {
            templateBreakdownSheet.Cell(templateRow, 1).Value = row.TemplateType.ToString();
            templateBreakdownSheet.Cell(templateRow, 2).Value = row.CampaignCount;
            templateBreakdownSheet.Cell(templateRow, 3).Value = row.ClickCount;
            templateBreakdownSheet.Cell(templateRow, 4).Value = row.ConversionCount;
            templateRow++;
        }

        kpiSheet.Columns().AdjustToContents();
        trendsSheet.Columns().AdjustToContents();
        campaignBreakdownSheet.Columns().AdjustToContents();
        templateBreakdownSheet.Columns().AdjustToContents();

        return Task.FromResult(ToExportFile(workbook, $"analytics-report-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.xlsx"));
    }

    private static void BuildCampaignsTable(IXLWorksheet worksheet, IReadOnlyCollection<Campaign> campaigns)
    {
        worksheet.Cell(1, 1).Value = "CampaignId";
        worksheet.Cell(1, 2).Value = "Name";
        worksheet.Cell(1, 3).Value = "TemplateType";
        worksheet.Cell(1, 4).Value = "Status";
        worksheet.Cell(1, 5).Value = "StartDateUtc";
        worksheet.Cell(1, 6).Value = "EndDateUtc";
        worksheet.Cell(1, 7).Value = "DurationDays";
        StyleHeader(worksheet.Range(1, 1, 1, 7));

        int row = 2;
        foreach (Campaign campaign in campaigns)
        {
            worksheet.Cell(row, 1).Value = campaign.Id.ToString();
            worksheet.Cell(row, 2).Value = campaign.Name;
            worksheet.Cell(row, 3).Value = campaign.TemplateType.ToString();
            worksheet.Cell(row, 4).Value = campaign.Status.ToString();
            worksheet.Cell(row, 5).Value = campaign.StartDate.UtcDateTime;
            worksheet.Cell(row, 5).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            worksheet.Cell(row, 6).Value = campaign.EndDate.UtcDateTime;
            worksheet.Cell(row, 6).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            worksheet.Cell(row, 7).Value = Math.Round((campaign.EndDate - campaign.StartDate).TotalDays, 2);
            row++;
        }
    }

    private static void BuildReportTitle(IXLWorksheet worksheet, string title)
    {
        worksheet.Cell(1, 1).Value = title;
        worksheet.Range(1, 1, 1, 4).Merge();
        worksheet.Range(1, 1, 1, 4).Style.Font.Bold = true;
        worksheet.Range(1, 1, 1, 4).Style.Font.FontSize = 16;
        worksheet.Range(1, 1, 1, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#EEF2FF");
    }

    private static void StyleHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#1D4ED8");
        range.Style.Font.FontColor = XLColor.White;
    }

    private static void SetCellValue(IXLCell cell, object value)
    {
        switch (value)
        {
            case int intValue:
                cell.Value = intValue;
                break;
            case long longValue:
                cell.Value = longValue;
                break;
            case decimal decimalValue:
                cell.Value = decimalValue;
                break;
            case double doubleValue:
                cell.Value = doubleValue;
                break;
            case float floatValue:
                cell.Value = floatValue;
                break;
            case bool boolValue:
                cell.Value = boolValue;
                break;
            case DateTime dateTimeValue:
                cell.Value = dateTimeValue;
                break;
            case DateTimeOffset dateTimeOffsetValue:
                cell.Value = dateTimeOffsetValue.UtcDateTime;
                break;
            case string stringValue:
                cell.Value = stringValue;
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }

    private static ExportBinaryFile ToExportFile(XLWorkbook workbook, string fileName)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return new ExportBinaryFile(fileName, ExcelContentType, stream.ToArray());
    }
}
