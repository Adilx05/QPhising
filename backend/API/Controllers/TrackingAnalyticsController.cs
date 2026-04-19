using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.Contracts.Abstractions.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
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
}
