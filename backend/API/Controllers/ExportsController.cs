using System.Security.Claims;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QPhising.Application.Common;
using QPhising.Application.Common.Contracts.Exports;
using QPhising.Application.Features.Exports.DownloadExportFile;
using QPhising.Application.Features.Exports.GetExportJobStatus;
using QPhising.Application.Features.Exports.QueueExportJob;
using QPhising.Domain.Exports;

namespace QPhising.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class ExportsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.Viewer)]
    [ProducesResponseType(typeof(ExportJobContract), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Queue([FromBody] QueueExportJobApiRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Problem(
                title: "Export request failed",
                detail: "Authenticated user ID claim is missing.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        QueueExportJobCommand command = new(userId!, request.ExportType, request.Format, request.CorrelationId);

        Result<ExportJobContract> result = await mediator.Send(command, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return ToProblem(result, "Export request failed");
        }

        return Created($"/api/exports/{result.Value.Id}", result.Value);
    }

    [HttpGet("{exportJobId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.Viewer)]
    [ProducesResponseType(typeof(ExportJobContract), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(Guid exportJobId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Problem(
                title: "Export status request failed",
                detail: "Authenticated user ID claim is missing.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        GetExportJobStatusQuery query = new(exportJobId, userId!, User.IsInRole(AuthorizationPolicies.Admin));
        Result<ExportJobContract> result = await mediator.Send(query, cancellationToken);

        if (result.IsSuccess && result.Value is not null)
        {
            return Ok(result.Value);
        }

        if (ContainsForbidden(result))
        {
            return Forbid();
        }

        return ToProblem(result, "Export status request failed", treatNotFoundAs404: true);
    }

    [HttpGet("{exportJobId:guid}/download")]
    [Authorize(Policy = AuthorizationPolicies.Viewer)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Download(Guid exportJobId, CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Problem(
                title: "Export download failed",
                detail: "Authenticated user ID claim is missing.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        DownloadExportFileQuery query = new(exportJobId, userId!, User.IsInRole(AuthorizationPolicies.Admin));
        Result<DownloadExportFileResponse> result = await mediator.Send(query, cancellationToken);

        if (result.IsSuccess && result.Value is not null)
        {
            return File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
        }

        if (ContainsForbidden(result))
        {
            return Forbid();
        }

        return ToProblem(result, "Export download failed", treatNotFoundAs404: true);
    }

    private bool TryGetCurrentUserId(out string? userId)
    {
        userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                 ?? User.FindFirstValue("sub")
                 ?? User.Identity?.Name;

        return !string.IsNullOrWhiteSpace(userId);
    }

    private static bool ContainsForbidden<T>(Result<T> result)
    {
        return result.Errors.Any(error => string.Equals(error, "forbidden", StringComparison.OrdinalIgnoreCase));
    }

    private ObjectResult ToProblem<T>(Result<T> result, string title, bool treatNotFoundAs404 = false)
    {
        var errors = result.Errors.Count == 0 ? ["Request failed."] : result.Errors;

        bool notFound = treatNotFoundAs404 && errors.Any(error =>
            error.Contains("not found", StringComparison.OrdinalIgnoreCase));

        return Problem(
            title: title,
            detail: string.Join("; ", errors),
            statusCode: notFound ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest)!;
    }
}

public sealed record QueueExportJobApiRequest(
    ExportType ExportType,
    ExportFormat Format,
    string? CorrelationId = null);
