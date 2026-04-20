using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.Contracts.Abstractions.Reporting;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.CQRS.Queries.Reporting;
using QPhising.Application.CQRS.Queries.Tracking;
using QPhising.Application.Security;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/tracking/analytics")]
[Produces("application/json", "application/problem+json")]
public sealed class TrackingAnalyticsController : ControllerBase
{
    private readonly ISender _sender;

    public TrackingAnalyticsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("overview", Name = "TrackingAnalytics_GetOverview")]
    [Authorize(Policy = IdentityAuthorizationPolicies.ViewerOrAbove)]
    [ProducesResponseType(typeof(TrackingAnalyticsOverviewResult), StatusCodes.Status200OK)]
    public Task<TrackingAnalyticsOverviewResult> GetOverview(
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery] TrackingVisitTrendBucketWindow trendWindow = TrackingVisitTrendBucketWindow.Day,
        [FromQuery] int timezoneOffsetMinutes = 0,
        [FromQuery] bool excludeBots = true,
        [FromQuery] int topPagesLimit = 10,
        [FromQuery] int recentVisitLimit = 25,
        CancellationToken cancellationToken = default) =>
        _sender.Send(
            new GetTrackingAnalyticsOverviewQuery(
                fromUtc,
                toUtc,
                trendWindow,
                timezoneOffsetMinutes,
                excludeBots,
                topPagesLimit,
                recentVisitLimit),
            cancellationToken);

    [HttpGet("reports/export", Name = "TrackingAnalytics_ExportReport")]
    [Authorize(Policy = IdentityAuthorizationPolicies.ViewerOrAbove)]
    [Produces("application/pdf", "text/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<FileResult> ExportReport(
        [FromQuery] TrackingReportFormat format = TrackingReportFormat.Csv,
        [FromQuery] TrackingReportScope scope = TrackingReportScope.Global,
        [FromQuery] TrackingReportDetailLevel detailLevel = TrackingReportDetailLevel.Summary,
        [FromQuery] Guid? trackingPageId = null,
        [FromQuery] DateTimeOffset? fromUtc = null,
        [FromQuery] DateTimeOffset? toUtc = null,
        [FromQuery] bool excludeBots = true,
        [FromQuery] int timezoneOffsetMinutes = 0,
        [FromQuery] string language = "en",
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(
            new ExportTrackingAnalyticsReportQuery(
                format,
                scope,
                detailLevel,
                trackingPageId,
                fromUtc,
                toUtc,
                excludeBots,
                timezoneOffsetMinutes,
                language),
            cancellationToken);

        return File(result.Content, result.ContentType, result.FileName);
    }
}
