using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QPhising.Api.Security;
using QPhising.Api.Contracts.Tracking;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.CQRS.Commands.Tracking;
using QPhising.Application.CQRS.Queries.Tracking;
using QPhising.Application.Security;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/tracking/pages")]
[Produces("application/json", "application/problem+json")]
public sealed class TrackingController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IVisitorIpHashService _visitorIpHashService;
    private readonly ILogger<TrackingController> _logger;

    public TrackingController(ISender sender, IVisitorIpHashService visitorIpHashService, ILogger<TrackingController> logger)
    {
        _sender = sender;
        _visitorIpHashService = visitorIpHashService;
        _logger = logger;
    }

    [HttpGet(Name = "TrackingPage_List")]
    [Authorize(Policy = IdentityAuthorizationPolicies.ViewerOrAbove)]
    [ProducesResponseType(typeof(IReadOnlyCollection<TrackingPageResult>), StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<TrackingPageResult>> List(CancellationToken cancellationToken) =>
        _sender.Send(new ListTrackingPagesQuery(), cancellationToken);

    [HttpGet("{trackingPageId:guid}", Name = "TrackingPage_GetById")]
    [Authorize(Policy = IdentityAuthorizationPolicies.ViewerOrAbove)]
    [ProducesResponseType(typeof(TrackingPageResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<TrackingPageResult> GetById([FromRoute] Guid trackingPageId, CancellationToken cancellationToken) =>
        _sender.Send(new GetTrackingPageByIdQuery(trackingPageId), cancellationToken);

    [HttpPost(Name = "TrackingPage_Create")]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TrackingPageResult), StatusCodes.Status200OK)]
    public Task<TrackingPageResult> Create([FromBody] CreateTrackingPageRequest request, CancellationToken cancellationToken) =>
        _sender.Send(
            new CreateTrackingPageCommand(
                request.Slug,
                request.Title,
                request.Description,
                request.OwnerId,
                request.TemplateId,
                request.CustomHtmlContent,
                request.ValidFromUtc,
                request.ValidUntilUtc,
                request.RetentionDays,
                request.MaskIpAddress,
                request.EnableBotFiltering,
                request.CaptureUtmParameters),
            cancellationToken);

    [HttpPut("{trackingPageId:guid}", Name = "TrackingPage_Update")]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TrackingPageResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<TrackingPageResult> Update(
        [FromRoute] Guid trackingPageId,
        [FromBody] UpdateTrackingPageRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(
            new UpdateTrackingPageCommand(
                trackingPageId,
                request.Slug,
                request.Title,
                request.Description,
                request.TemplateId,
                request.CustomHtmlContent,
                request.ValidFromUtc,
                request.ValidUntilUtc,
                request.CustomHtmlContent,
                request.ValidFromUtc,
                request.ValidUntilUtc,
                request.RetentionDays,
                request.MaskIpAddress,
                request.EnableBotFiltering,
                request.CaptureUtmParameters),
            cancellationToken);

    [HttpPost("{trackingPageId:guid}/publish", Name = "TrackingPage_Publish")]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    [ProducesResponseType(typeof(TrackingPageResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<TrackingPageResult> Publish([FromRoute] Guid trackingPageId, CancellationToken cancellationToken) =>
        _sender.Send(new PublishTrackingPageCommand(trackingPageId), cancellationToken);

    [HttpPost("{trackingPageId:guid}/archive", Name = "TrackingPage_Archive")]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    [ProducesResponseType(typeof(TrackingPageResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<TrackingPageResult> Archive([FromRoute] Guid trackingPageId, CancellationToken cancellationToken) =>
        _sender.Send(new ArchiveTrackingPageCommand(trackingPageId), cancellationToken);

    [HttpDelete("{trackingPageId:guid}", Name = "TrackingPage_Delete")]
    [Authorize(Policy = IdentityAuthorizationPolicies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid trackingPageId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteTrackingPageCommand(trackingPageId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{trackingPageId:guid}/visits", Name = "TrackingPage_CaptureVisit")]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.PublicVisitIngestion)]
    [RequestSizeLimit(16 * 1024)]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(VisitIngestionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<VisitIngestionResult> CaptureVisit(
        [FromRoute] Guid trackingPageId,
        [FromBody] CaptureVisitRequest request,
        CancellationToken cancellationToken)
    {
        var resolvedIpHash = _visitorIpHashService.ResolveHash(HttpContext.Connection.RemoteIpAddress, request.IpAddressHashPolicy);
        if (!string.IsNullOrWhiteSpace(request.IpHash))
        {
            _logger.LogInformation(
                "Ignored client-supplied IP hash for tracking page {TrackingPageId}. Policy {Policy} uses server-side hashing.",
                trackingPageId,
                request.IpAddressHashPolicy);
        }

        return _sender.Send(
            new IngestVisitEventCommand(
                trackingPageId,
                request.OccurredAtUtc,
                request.SessionId,
                request.VisitorFingerprint,
                request.UserAgent,
                request.ReferrerUrl,
                resolvedIpHash,
                request.IpAddressHashPolicy,
                request.DeduplicationWindowSeconds),
            cancellationToken);
    }

    [HttpGet("{trackingPageId:guid}/analytics", Name = "TrackingPage_GetAnalytics")]
    [Authorize(Policy = IdentityAuthorizationPolicies.ViewerOrAbove)]
    [ProducesResponseType(typeof(TrackingPageAnalyticsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<TrackingPageAnalyticsResult> GetAnalytics(
        [FromRoute] Guid trackingPageId,
        [FromQuery] DateTimeOffset? fromUtc,
        [FromQuery] DateTimeOffset? toUtc,
        [FromQuery] int trendBucketSizeMinutes = 60,
        [FromQuery] int recentVisitLimit = 25,
        CancellationToken cancellationToken = default) =>
        _sender.Send(
            new GetTrackingPageAnalyticsQuery(trackingPageId, fromUtc, toUtc, trendBucketSizeMinutes, recentVisitLimit),
            cancellationToken);
}
