using MediatR;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.Contracts.Responses.Tracking;
using QPhising.Application.CQRS.Queries.Tracking;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("p")]
[Produces("application/json", "application/problem+json")]
public sealed class PublicTrackingController : ControllerBase
{
    private readonly ISender _sender;

    public PublicTrackingController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("{slug}", Name = "Tracking_PublicLandingBySlug")]
    [ProducesResponseType(typeof(TrackingLandingPageResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<TrackingLandingPageResult> ResolveLandingPage([FromRoute] string slug, CancellationToken cancellationToken) =>
        _sender.Send(new GetTrackingLandingPageBySlugQuery(slug), cancellationToken);
}
