using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Api.Contracts.Template;
using QPhising.Application.Contracts.Responses.Template;
using QPhising.Application.CQRS.Commands.Template;
using QPhising.Application.CQRS.Queries.Template;
using QPhising.Application.Security;

namespace QPhising.Api.Controllers;

[ApiController]
[Route("api/templates")]
[Produces("application/json", "application/problem+json")]
[Authorize(Policy = IdentityAuthorizationPolicies.ViewerOrAbove)]
public sealed class TemplateController : ControllerBase
{
    private readonly ISender _sender;

    public TemplateController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet(Name = "Template_List")]
    [ProducesResponseType(typeof(IReadOnlyCollection<TemplateResult>), StatusCodes.Status200OK)]
    public Task<IReadOnlyCollection<TemplateResult>> List(CancellationToken cancellationToken) =>
        _sender.Send(new ListTemplatesQuery(), cancellationToken);

    [HttpGet("{templateId:guid}", Name = "Template_GetById")]
    [ProducesResponseType(typeof(TemplateResult), StatusCodes.Status200OK)]
    public Task<TemplateResult> GetById([FromRoute] Guid templateId, CancellationToken cancellationToken) =>
        _sender.Send(new GetTemplateByIdQuery(templateId), cancellationToken);

    [HttpPost(Name = "Template_Create")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TemplateResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<TemplateResult> Create([FromBody] CreateTemplateRequest request, CancellationToken cancellationToken) =>
        _sender.Send(new CreateTemplateCommand(request.Name, request.HtmlContent, request.Description, request.Tags), cancellationToken);

    [HttpPut("{templateId:guid}", Name = "Template_Update")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TemplateResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<TemplateResult> Update(
        [FromRoute] Guid templateId,
        [FromBody] UpdateTemplateRequest request,
        CancellationToken cancellationToken) =>
        _sender.Send(new UpdateTemplateCommand(templateId, request.Name, request.HtmlContent, request.Description, request.Tags), cancellationToken);

    [HttpPost("{templateId:guid}/publish", Name = "Template_Publish")]
    [ProducesResponseType(typeof(TemplateResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<TemplateResult> Publish([FromRoute] Guid templateId, CancellationToken cancellationToken) =>
        _sender.Send(new PublishTemplateCommand(templateId), cancellationToken);

    [HttpPost("{templateId:guid}/archive", Name = "Template_Archive")]
    [ProducesResponseType(typeof(TemplateResult), StatusCodes.Status200OK)]
    [Authorize(Policy = IdentityAuthorizationPolicies.OperatorOrAbove)]
    public Task<TemplateResult> Archive([FromRoute] Guid templateId, CancellationToken cancellationToken) =>
        _sender.Send(new ArchiveTemplateCommand(templateId), cancellationToken);

    [HttpDelete("{templateId:guid}", Name = "Template_Delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Authorize(Policy = IdentityAuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> Delete([FromRoute] Guid templateId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteTemplateCommand(templateId), cancellationToken);
        return NoContent();
    }
}
