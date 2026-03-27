using AutoMapper;
using MediatR;
using QPhising.Application.Common;
using QPhising.Application.Common.Contracts.Exports;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Exports;
using QPhising.Domain.Tasks;

namespace QPhising.Application.Features.Exports.QueueExportJob;

public sealed class QueueExportJobCommandHandler(
    IExportJobRepository exportJobRepository,
    IQueuedTaskRepository queuedTaskRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<QueueExportJobCommand, Result<ExportJobContract>>
{
    private const int ExportTaskMaxAttempts = 5;

    public async Task<Result<ExportJobContract>> Handle(QueueExportJobCommand request, CancellationToken cancellationToken)
    {
        ExportJob exportJob = ExportJob.Create(
            request.OwnerUserId,
            request.ExportType,
            request.Format,
            correlationId: request.CorrelationId);

        exportJob.Queue();

        IReadOnlyDictionary<string, string> taskPayload = new Dictionary<string, string>
        {
            ["exportJobId"] = exportJob.Id.ToString(),
            ["exportType"] = request.ExportType.ToString(),
            ["format"] = request.Format.ToString(),
            ["requestedByUserId"] = request.OwnerUserId,
            ["requestedAt"] = exportJob.RequestedAt.ToString("O")
        };

        QueuedTask exportTask = QueuedTask.Create(
            TaskType.ExportGeneration,
            taskPayload,
            maxAttempts: ExportTaskMaxAttempts,
            correlationId: request.CorrelationId);

        await exportJobRepository.AddAsync(exportJob, cancellationToken);
        await queuedTaskRepository.AddAsync(exportTask, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ExportJobContract contract = mapper.Map<ExportJobContract>(exportJob);
        return Result<ExportJobContract>.Success(contract);
    }
}
