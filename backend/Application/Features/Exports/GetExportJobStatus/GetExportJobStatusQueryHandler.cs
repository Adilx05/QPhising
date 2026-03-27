using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Contracts.Exports;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Exports;

namespace QPhising.Application.Features.Exports.GetExportJobStatus;

public sealed class GetExportJobStatusQueryHandler(
    IExportJobRepository exportJobRepository,
    IMapper mapper) : IRequestHandler<GetExportJobStatusQuery, Result<ExportJobContract>>
{
    public async Task<Result<ExportJobContract>> Handle(GetExportJobStatusQuery request, CancellationToken cancellationToken)
    {
        ExportJob? exportJob = await exportJobRepository.GetByIdAsync(request.ExportJobId, cancellationToken);
        if (exportJob is null)
        {
            return Result<ExportJobContract>.Failure($"Export job '{request.ExportJobId}' was not found.");
        }

        if (!request.IsAdmin && !string.Equals(exportJob.OwnerUserId, request.RequestingUserId, StringComparison.Ordinal))
        {
            return Result<ExportJobContract>.Failure("forbidden");
        }

        ExportJobContract contract = mapper.Map<ExportJobContract>(exportJob);
        return Result<ExportJobContract>.Success(contract);
    }
}
