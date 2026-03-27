using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Common.Abstractions.Exports;

public interface IExcelExportService
{
    Task<ExportBinaryFile> BuildCampaignReportAsync(
        IReadOnlyCollection<Campaign> campaigns,
        CancellationToken cancellationToken = default);

    Task<ExportBinaryFile> BuildAnalyticsReportAsync(
        AnalyticsDashboardReadModel dashboard,
        AnalyticsReadCriteria criteria,
        CancellationToken cancellationToken = default);
}

public sealed record ExportBinaryFile(
    string FileName,
    string ContentType,
    byte[] Content);
