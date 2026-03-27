using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.Common;
using QPhising.Application.Features.Campaigns.ActivateCampaign;
using QPhising.Application.Features.Campaigns.CreateCampaign;
using QPhising.Application.Features.Campaigns.GetCampaignById;
using QPhising.Application.Features.Campaigns.ListCampaigns;
using QPhising.Application.Features.Campaigns.ScheduleCampaign;
using QPhising.Application.Features.Campaigns.UpdateCampaign;
using QPhising.Domain.Campaigns;

namespace QPhising.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class CampaignsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.Viewer)]
    [ProducesResponseType(typeof(ListCampaignsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] CampaignStatus[]? statuses,
        [FromQuery] TemplateType[]? templateTypes,
        [FromQuery] DateTimeOffset? startsOnOrAfter,
        [FromQuery] DateTimeOffset? endsOnOrBefore,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        ListCampaignsQuery query = new(
            statuses,
            templateTypes,
            startsOnOrAfter,
            endsOnOrBefore,
            skip,
            take);

        Result<ListCampaignsResponse> result = await mediator.Send(query, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{campaignId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Viewer)]
    [ProducesResponseType(typeof(CampaignDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid campaignId, CancellationToken cancellationToken)
    {
        Result<CampaignDetailResponse> result = await mediator.Send(new GetCampaignByIdQuery(campaignId), cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Operator)]
    [ProducesResponseType(typeof(CreateCampaignResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCampaignRequest request, CancellationToken cancellationToken)
    {
        CreateCampaignCommand command = new(
            request.Name,
            request.TemplateType,
            request.HtmlContent,
            request.StartDate,
            request.EndDate);

        Result<CreateCampaignResponse> result = await mediator.Send(command, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return ToActionResult(result);
        }

        return Created($"/api/campaigns/{result.Value.Id}", result.Value);
    }

    [HttpPut("{campaignId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Operator)]
    [ProducesResponseType(typeof(UpdateCampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid campaignId, [FromBody] UpdateCampaignRequest request, CancellationToken cancellationToken)
    {
        UpdateCampaignCommand command = new(
            campaignId,
            request.Name,
            request.TemplateType,
            request.HtmlContent,
            request.StartDate,
            request.EndDate);

        Result<UpdateCampaignResponse> result = await mediator.Send(command, cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{campaignId:guid}/schedule")]
    [Authorize(Policy = AuthorizationPolicies.Operator)]
    [ProducesResponseType(typeof(ScheduleCampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Schedule(Guid campaignId, CancellationToken cancellationToken)
    {
        Result<ScheduleCampaignResponse> result = await mediator.Send(new ScheduleCampaignCommand(campaignId), cancellationToken);
        return ToActionResult(result);
    }

    [HttpPost("{campaignId:guid}/activate")]
    [Authorize(Policy = AuthorizationPolicies.Operator)]
    [ProducesResponseType(typeof(ActivateCampaignResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid campaignId, CancellationToken cancellationToken)
    {
        Result<ActivateCampaignResponse> result = await mediator.Send(new ActivateCampaignCommand(campaignId), cancellationToken);
        return ToActionResult(result);
    }

    private IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return Ok(result.Value);
        }

        var errors = result.Errors.Count == 0 ? new[] { "Request failed." } : result.Errors;
        bool notFound = errors.Any(error =>
            error.Contains("not found", StringComparison.OrdinalIgnoreCase));

        return Problem(
            title: notFound ? "Campaign not found" : "Campaign request failed",
            detail: string.Join("; ", errors),
            statusCode: notFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest);
    }
}

public sealed record CreateCampaignRequest(
    string Name,
    TemplateType TemplateType,
    string HtmlContent,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);

public sealed record UpdateCampaignRequest(
    string Name,
    TemplateType TemplateType,
    string HtmlContent,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate);
