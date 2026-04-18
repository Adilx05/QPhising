using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Api.Contracts.Campaign;
using QPhising.Application.Contracts.Responses.Campaign;
using QPhising.Application.CQRS.Commands.Campaign;
using QPhising.Application.CQRS.Queries.Campaign;
using QPhising.Application.Security;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/campaigns")]
[Produces("application/json", "application/problem+json")]
[Authorize(Policy = IdentityAuthorizationPolicies.ViewerOrAbove)]
public sealed class CampaignController : ControllerBase
{
    private readonly ISender _sender;

    public CampaignController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet(Name = "Campaign_List")]
    [ProducesResponseType(typeof(IReadOnlyCollection<CampaignResult>), StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<CampaignResult>> List(CancellationToken cancellationToken) =>
        _sender.Send(new ListCampaignsQuery(), cancellationToken);

    [HttpGet("{campaignId:guid}", Name = "Campaign_GetById")]
    [ProducesResponseType(typeof(CampaignResult), StatusCodes.Status200OK)]
    public Task<CampaignResult> GetById([FromRoute] Guid campaignId, CancellationToken cancellationToken) =>
        _sender.Send(new GetCampaignByIdQuery(campaignId), cancellationToken);

    [HttpPost(Name = "Campaign_Create")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CampaignResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<CampaignResult> Create([FromBody] CreateCampaignRequest request, CancellationToken cancellationToken) =>
        _sender.Send(new CreateCampaignCommand(request.Name, request.TemplateId), cancellationToken);

    [HttpPut("{campaignId:guid}", Name = "Campaign_Update")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CampaignResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<CampaignResult> Update(
        [FromRoute] Guid campaignId,
        [FromBody] UpdateCampaignRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(new UpdateCampaignCommand(campaignId, request.Name), cancellationToken);

    [HttpDelete("{campaignId:guid}", Name = "Campaign_Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = IdentityAuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Delete([FromRoute] Guid campaignId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteCampaignCommand(campaignId), cancellationToken);
        return NoContent();
    }

    [HttpPost("{campaignId:guid}/targets", Name = "Campaign_AddTarget")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CampaignResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<CampaignResult> AddTarget(
        [FromRoute] Guid campaignId,
        [FromBody] AddCampaignTargetRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(new AddCampaignTargetCommand(campaignId, request.EmailAddress), cancellationToken);

    [HttpDelete("{campaignId:guid}/targets/{targetId:guid}", Name = "Campaign_RemoveTarget")]
    [ProducesResponseType(typeof(CampaignResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<CampaignResult> RemoveTarget(
        [FromRoute] Guid campaignId,
        [FromRoute] Guid targetId,
        CancellationToken cancellationToken) =>
        _sender.Send(new RemoveCampaignTargetCommand(campaignId, targetId), cancellationToken);

    [HttpPost("{campaignId:guid}/schedule", Name = "Campaign_Schedule")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(CampaignResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<CampaignResult> Schedule(
        [FromRoute] Guid campaignId,
        [FromBody] ScheduleCampaignRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(new ScheduleCampaignCommand(campaignId, request.StartsAtUtc, request.EndsAtUtc), cancellationToken);

    [HttpPost("{campaignId:guid}/start", Name = "Campaign_Start")]
    [ProducesResponseType(typeof(CampaignResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<CampaignResult> Start([FromRoute] Guid campaignId, CancellationToken cancellationToken) =>
        _sender.Send(new StartCampaignCommand(campaignId), cancellationToken);

    [HttpPost("{campaignId:guid}/pause", Name = "Campaign_Pause")]
    [ProducesResponseType(typeof(CampaignResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<CampaignResult> Pause([FromRoute] Guid campaignId, CancellationToken cancellationToken) =>
        _sender.Send(new PauseCampaignCommand(campaignId), cancellationToken);

    [HttpPost("{campaignId:guid}/complete", Name = "Campaign_Complete")]
    [ProducesResponseType(typeof(CampaignResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<CampaignResult> Complete([FromRoute] Guid campaignId, CancellationToken cancellationToken) =>
        _sender.Send(new CompleteCampaignCommand(campaignId), cancellationToken);

    [HttpPost("{campaignId:guid}/cancel", Name = "Campaign_Cancel")]
    [ProducesResponseType(typeof(CampaignResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.AdminOnly)]
    public Task<CampaignResult> Cancel([FromRoute] Guid campaignId, CancellationToken cancellationToken) =>
        _sender.Send(new CancelCampaignCommand(campaignId), cancellationToken);
}
