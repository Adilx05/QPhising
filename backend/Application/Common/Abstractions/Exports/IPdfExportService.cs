using QPhising.Application.Common.Abstractions;
using QPhising.Domain.Campaigns;

namespace QPhising.Application.Common.Abstractions.Exports;

public interface IPdfExportService
{
    Task<ExportBinaryFile> BuildCampaignReportAsync(
        IReadOnlyCollection<Campaign> campaigns,
        CancellationToken cancellationToken = default);

    Task<ExportBinaryFile> BuildAnalyticsReportAsync(
        AnalyticsDashboardReadModel dashboard,
        AnalyticsReadCriteria criteria,
        CancellationToken cancellationToken = default);
}
