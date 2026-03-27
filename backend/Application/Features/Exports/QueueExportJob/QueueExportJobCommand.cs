using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Contracts.Exports;
using QPhising.Domain.Exports;

namespace QPhising.Application.Features.Exports.QueueExportJob;

public sealed record QueueExportJobCommand(
    string OwnerUserId,
    ExportType ExportType,
    ExportFormat Format,
    string? CorrelationId = null) : IRequest<Result<ExportJobContract>>;
