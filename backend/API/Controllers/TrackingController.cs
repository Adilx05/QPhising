using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QPhising.API.Configuration;
using QPhising.Application.Common;
using QPhising.Application.Features.Tracking.GenerateTrackingLink;
using QPhising.Application.Features.Tracking.ProcessTrackingClick;

namespace QPhising.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class TrackingController(IMediator mediator, IOptions<BaseUrlOptions> baseUrlOptions) : ControllerBase
{
    [HttpPost("links")]
    [Authorize(Policy = AuthorizationPolicies.Operator)]
    [ProducesResponseType(typeof(GenerateTrackingLinkApiResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateTrackingLink([FromBody] GenerateTrackingLinkApiRequest request, CancellationToken cancellationToken)
    {
        GenerateTrackingLinkCommand command = new(request.CampaignId, request.RecipientEmail);
        Result<GenerateTrackingLinkResponse> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return ToActionResult(result);
        }

        var gatewayBaseUrl = baseUrlOptions.Value.Gateway.TrimEnd('/');
        GenerateTrackingLinkApiResponse response = new(
            result.Value.CampaignId,
            result.Value.RecipientEmail,
            result.Value.TrackingToken,
            $"{gatewayBaseUrl}{result.Value.TrackingPath}",
            result.Value.GeneratedAtUtc,
            result.Value.ExpiresAtUtc,
            result.Value.SignatureAlgorithm,
            result.Value.TokenVersion);

        return Created(response.TrackingUrl, response);
    }

    [HttpGet("click/{campaignId:guid}/{trackingToken}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProcessTrackingClickApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ProcessTrackingClick(
        Guid campaignId,
        string trackingToken,
        [FromQuery] string? fingerprint,
        CancellationToken cancellationToken)
    {
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        string userAgent = Request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            userAgent = "unknown";
        }

        ProcessTrackingClickCommand command = new(campaignId, trackingToken, ipAddress, userAgent, fingerprint);
        Result<ProcessTrackingClickResponse> result = await mediator.Send(command, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return ToActionResult(result);
        }

        ProcessTrackingClickApiResponse response = new(
            result.Value.ClickId,
            result.Value.CampaignId,
            result.Value.ClickedAtUtc,
            result.Value.IpAddress,
            result.Value.UserAgent,
            result.Value.Fingerprint,
            result.Value.Accepted);

        return Ok(response);
    }

    private IActionResult ToActionResult<T>(Result<T> result)
    {
        var errors = result.Errors.Count == 0 ? new[] { "Tracking request failed." } : result.Errors;
        bool notFound = errors.Any(error => error.Contains("not found", StringComparison.OrdinalIgnoreCase));

        return Problem(
            title: notFound ? "Campaign not found" : "Tracking request failed",
            detail: string.Join("; ", errors),
            statusCode: notFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest);
    }
}

public sealed record GenerateTrackingLinkApiRequest(Guid CampaignId, string RecipientEmail);

public sealed record GenerateTrackingLinkApiResponse(
    Guid CampaignId,
    string RecipientEmail,
    string TrackingToken,
    string TrackingUrl,
    DateTimeOffset GeneratedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    string SignatureAlgorithm,
    int TokenVersion);

public sealed record ProcessTrackingClickApiResponse(
    Guid ClickId,
    Guid CampaignId,
    DateTimeOffset ClickedAtUtc,
    string IpAddress,
    string UserAgent,
    string? Fingerprint,
    bool Accepted);
