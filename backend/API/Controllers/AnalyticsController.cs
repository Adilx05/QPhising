using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.Common;
using QPhising.Application.Features.Analytics.GetDashboardKpis;
using QPhising.Domain.Campaigns;

namespace QPhising.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AnalyticsController(IMediator mediator) : ControllerBase
{
    [HttpGet("dashboard-kpis")]
    [Authorize(Policy = AuthorizationPolicies.Viewer)]
    [ProducesResponseType(typeof(DashboardKpisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDashboardKpis(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        [FromQuery] AnalyticsTimeGrain timeGrain = AnalyticsTimeGrain.Day,
        [FromQuery] string timeZone = "UTC",
        [FromQuery] Guid[]? campaignIds = null,
        [FromQuery] TemplateType[]? templateTypes = null,
        [FromQuery] CampaignStatus[]? campaignStatuses = null,
        CancellationToken cancellationToken = default)
    {
        GetDashboardKpisQuery query = new(
            from,
            to,
            timeGrain,
            timeZone,
            campaignIds,
            templateTypes,
            campaignStatuses);

        Result<DashboardKpisResponse> result = await mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult(Result<DashboardKpisResponse> result)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return Ok(result.Value);
        }

        var errors = result.Errors.Count == 0 ? new[] { "Analytics request failed." } : result.Errors;

        return Problem(
            title: "Analytics request failed",
            detail: string.Join("; ", errors),
            statusCode: StatusCodes.Status400BadRequest);
    }
}
