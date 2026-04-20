using MediatR;
using QPhising.Application.Contracts.Abstractions.Reporting;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Reporting;

namespace QPhising.Application.CQRS.Queries.Reporting;

public sealed class ExportTrackingAnalyticsReportQueryHandler : IRequestHandler<ExportTrackingAnalyticsReportQuery, TrackingReportFileResult>
{
    private readonly ITrackingPageRepository _trackingPageRepository;
    private readonly IVisitEventRepository _visitEventRepository;
    private readonly ITrackingReportExporter _trackingReportExporter;

    public ExportTrackingAnalyticsReportQueryHandler(
        ITrackingPageRepository trackingPageRepository,
        IVisitEventRepository visitEventRepository,
        ITrackingReportExporter trackingReportExporter)
    {
        _trackingPageRepository = trackingPageRepository;
        _visitEventRepository = visitEventRepository;
        _trackingReportExporter = trackingReportExporter;
    }

    public async Task<TrackingReportFileResult> Handle(ExportTrackingAnalyticsReportQuery request, CancellationToken cancellationToken)
    {
        var fromUtc = request.FromUtc;
        var toUtc = request.ToUtc;

        var data = request.Scope switch
        {
            TrackingReportScope.Global => await BuildGlobalDataAsync(request, fromUtc, toUtc, cancellationToken),
            TrackingReportScope.TrackingPage => await BuildTrackingPageDataAsync(request, fromUtc, toUtc, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Scope), request.Scope, "Unsupported tracking report scope.")
        };

        var timestamp = DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmss");
        var scopeSegment = request.Scope == TrackingReportScope.Global ? "global" : "tracking-page";
        var detailSegment = request.DetailLevel == TrackingReportDetailLevel.Detailed ? "detailed" : "summary";

        return request.Format switch
        {
            TrackingReportFormat.Csv => new TrackingReportFileResult(
                ContentType: "text/csv",
                FileName: $"tracking-report-{scopeSegment}-{detailSegment}-{timestamp}.csv",
                Content: _trackingReportExporter.BuildCsv(data, request.Language)),
            TrackingReportFormat.Pdf => new TrackingReportFileResult(
                ContentType: "application/pdf",
                FileName: $"tracking-report-{scopeSegment}-{detailSegment}-{timestamp}.pdf",
                Content: _trackingReportExporter.BuildPdf(data, request.Language)),
            _ => throw new ArgumentOutOfRangeException(nameof(request.Format), request.Format, "Unsupported tracking report format.")
        };
    }

    private async Task<TrackingReportData> BuildGlobalDataAsync(
        ExportTrackingAnalyticsReportQuery request,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var totalVisits = await _visitEventRepository.CountTotalAcrossPagesAsync(fromUtc, toUtc, request.ExcludeBots, cancellationToken);
        var uniqueVisitors = await _visitEventRepository.CountUniqueVisitorsAcrossPagesAsync(fromUtc, toUtc, request.ExcludeBots, cancellationToken);
        var recentVisits = await _visitEventRepository.ListRecentAcrossPagesAsync(fromUtc, toUtc, request.ExcludeBots, 150, cancellationToken);
        var trends = await _visitEventRepository.GetTrendBucketsAcrossPagesAsync(
            fromUtc ?? DateTimeOffset.UtcNow.AddYears(-5),
            toUtc ?? DateTimeOffset.UtcNow,
            TrackingVisitTrendBucketWindow.Day,
            request.TimezoneOffsetMinutes,
            request.ExcludeBots,
            cancellationToken);

        var topPages = await _visitEventRepository.ListTopPagesAsync(fromUtc, toUtc, request.ExcludeBots, 15, cancellationToken);

        var visitorRows = request.DetailLevel == TrackingReportDetailLevel.Detailed
            ? await _visitEventRepository.ListVisitorClickStatsAsync(null, fromUtc, toUtc, request.ExcludeBots, 250, cancellationToken)
            : Array.Empty<TrackingVisitorClickStat>();

        var recentByReferrer = recentVisits
            .GroupBy(visit => string.IsNullOrWhiteSpace(visit.ReferrerUrl) ? "direct" : visit.ReferrerUrl!, StringComparer.OrdinalIgnoreCase)
            .Select(group => (Label: group.Key, Value: group.Count()))
            .OrderByDescending(entry => entry.Value)
            .Take(8)
            .ToArray();

        var notes = new List<string>
        {
            request.ExcludeBots ? "Bot traffic excluded" : "Bot traffic included",
            "Global scope includes all active tracking pages."
        };

        return new TrackingReportData(
            Title: "Tracking Analytics Export",
            ScopeLabel: "Global",
            FromUtc: fromUtc,
            ToUtc: toUtc,
            TotalVisits: totalVisits,
            UniqueVisitors: uniqueVisitors,
            LastVisitAtUtc: recentVisits.FirstOrDefault()?.OccurredAtUtc,
            TrendRows: trends.Select(trend => (trend.BucketStartUtc.ToString("yyyy-MM-dd"), trend.TotalVisits, trend.UniqueVisitors)).ToArray(),
            DistributionRows: topPages.Select(page => ($"/{page.Slug}", page.TotalVisits)).ToArray().Length > 0
                ? topPages.Select(page => ($"/{page.Slug}", page.TotalVisits)).ToArray()
                : recentByReferrer,
            VisitorRows: visitorRows,
            AppliedNotes: notes);
    }

    private async Task<TrackingReportData> BuildTrackingPageDataAsync(
        ExportTrackingAnalyticsReportQuery request,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var trackingPage = await _trackingPageRepository.GetByIdAsync(request.TrackingPageId!.Value, cancellationToken)
            ?? throw new KeyNotFoundException($"Tracking page '{request.TrackingPageId}' was not found.");

        var totalVisits = await _visitEventRepository.CountTotalAsync(trackingPage.Id, fromUtc, toUtc, cancellationToken);
        var uniqueVisitors = await _visitEventRepository.CountUniqueVisitorsAsync(trackingPage.Id, fromUtc, toUtc, cancellationToken);
        var lastVisitAtUtc = await _visitEventRepository.GetLastVisitAtUtcAsync(trackingPage.Id, cancellationToken);
        var trends = await _visitEventRepository.GetTrendBucketsAsync(
            trackingPage.Id,
            fromUtc ?? DateTimeOffset.UtcNow.AddDays(-30),
            toUtc ?? DateTimeOffset.UtcNow,
            60,
            cancellationToken);
        var recentVisits = await _visitEventRepository.ListRecentAsync(trackingPage.Id, fromUtc, toUtc, 250, cancellationToken);

        var visitorRows = request.DetailLevel == TrackingReportDetailLevel.Detailed
            ? await _visitEventRepository.ListVisitorClickStatsAsync(trackingPage.Id, fromUtc, toUtc, request.ExcludeBots, 250, cancellationToken)
            : Array.Empty<TrackingVisitorClickStat>();

        var deviceDistribution = recentVisits
            .GroupBy(visit => string.IsNullOrWhiteSpace(visit.UserAgent) ? "unknown" : visit.UserAgent!, StringComparer.OrdinalIgnoreCase)
            .Select(group => (Label: group.Key, Value: group.Count()))
            .OrderByDescending(entry => entry.Value)
            .Take(10)
            .ToArray();

        var notes = new List<string>
        {
            "Tracking-page scope export.",
            $"Slug: /{trackingPage.Slug.Value}"
        };

        return new TrackingReportData(
            Title: "Tracking Page Analytics Export",
            ScopeLabel: $"/{trackingPage.Slug.Value}",
            FromUtc: fromUtc,
            ToUtc: toUtc,
            TotalVisits: totalVisits,
            UniqueVisitors: uniqueVisitors,
            LastVisitAtUtc: lastVisitAtUtc,
            TrendRows: trends.Select(trend => (trend.BucketStartUtc.ToString("yyyy-MM-dd HH:mm"), trend.TotalVisits, trend.UniqueVisitors)).ToArray(),
            DistributionRows: deviceDistribution,
            VisitorRows: visitorRows,
            AppliedNotes: notes);
    }
}
