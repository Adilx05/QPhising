using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Contracts.Exports;

namespace QPhising.Application.Features.Exports.GetExportJobStatus;

public sealed record GetExportJobStatusQuery(
    Guid ExportJobId,
    string RequestingUserId,
    bool IsAdmin) : IRequest<Result<ExportJobContract>>;
