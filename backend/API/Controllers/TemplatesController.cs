using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.Common;
using QPhising.Application.Features.Templates.ArchiveTemplate;
using QPhising.Application.Features.Templates.CreateTemplate;
using QPhising.Application.Features.Templates.GetTemplateById;
using QPhising.Application.Features.Templates.ListTemplates;
using QPhising.Application.Features.Templates.PublishTemplate;
using QPhising.Application.Features.Templates.UpdateTemplate;
using QPhising.Domain.Templates;

namespace QPhising.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class TemplatesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.Viewer)]
    [ProducesResponseType(typeof(ListTemplatesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] TemplateStatus? status,
        [FromQuery] TemplateType? type,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        ListTemplatesQuery query = new(status, type, searchTerm, pageNumber, pageSize);

        Result<ListTemplatesResponse> result = await mediator.Send(query, cancellationToken);
        return ToActionResult(result, "Template list request failed");
    }

    [HttpGet("{templateId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Viewer)]
    [ProducesResponseType(typeof(TemplateDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid templateId, CancellationToken cancellationToken)
    {
        Result<TemplateDetailResponse> result = await mediator.Send(new GetTemplateByIdQuery(templateId), cancellationToken);
        return ToActionResult(result, "Template detail request failed");
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Operator)]
    [ProducesResponseType(typeof(CreateTemplateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTemplateRequest request, CancellationToken cancellationToken)
    {
        CreateTemplateCommand command = new(
            request.Name,
            request.Type,
            request.HtmlContent,
            request.Variables);

        Result<CreateTemplateResponse> result = await mediator.Send(command, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return ToActionResult(result, "Template creation failed");
        }

        return Created($"/api/templates/{result.Value.Id}", result.Value);
    }

    [HttpPut("{templateId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Operator)]
    [ProducesResponseType(typeof(UpdateTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid templateId, [FromBody] UpdateTemplateRequest request, CancellationToken cancellationToken)
    {
        UpdateTemplateCommand command = new(
            templateId,
            request.Name,
            request.Type,
            request.HtmlContent,
            request.Variables);

        Result<UpdateTemplateResponse> result = await mediator.Send(command, cancellationToken);
        return ToActionResult(result, "Template update failed");
    }

    [HttpPost("{templateId:guid}/publish")]
    [Authorize(Policy = AuthorizationPolicies.Operator)]
    [ProducesResponseType(typeof(PublishTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Publish(Guid templateId, CancellationToken cancellationToken)
    {
        Result<PublishTemplateResponse> result = await mediator.Send(new PublishTemplateCommand(templateId), cancellationToken);
        return ToActionResult(result, "Template publish failed");
    }

    [HttpPost("{templateId:guid}/archive")]
    [Authorize(Policy = AuthorizationPolicies.Operator)]
    [ProducesResponseType(typeof(ArchiveTemplateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid templateId, CancellationToken cancellationToken)
    {
        Result<ArchiveTemplateResponse> result = await mediator.Send(new ArchiveTemplateCommand(templateId), cancellationToken);
        return ToActionResult(result, "Template archive failed");
    }

    private IActionResult ToActionResult<T>(Result<T> result, string failureTitle)
    {
        if (result.IsSuccess && result.Value is not null)
        {
            return Ok(result.Value);
        }

        var errors = result.Errors.Count == 0 ? new[] { "Request failed." } : result.Errors;
        bool notFound = errors.Any(error =>
            error.Contains("not found", StringComparison.OrdinalIgnoreCase));

        return Problem(
            title: notFound ? "Template not found" : failureTitle,
            detail: string.Join("; ", errors),
            statusCode: notFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest);
    }
}

public sealed record CreateTemplateRequest(
    string Name,
    TemplateType Type,
    string HtmlContent,
    IReadOnlyCollection<string>? Variables = null);

public sealed record UpdateTemplateRequest(
    string Name,
    TemplateType Type,
    string HtmlContent,
    IReadOnlyCollection<string>? Variables = null);
