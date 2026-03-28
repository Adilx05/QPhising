using AutoMapper;
using QPhising.Application.Common.Contracts.Exports;
using QPhising.Application.Features.Exports.Common;
using QPhising.Application.Features.Exports.QueueExportJob;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Exports;
using QPhising.Domain.Tasks;
using Xunit;

namespace QPhising.API.IntegrationTests;

public sealed class QueueExportJobCommandHandlerTests
{
    private readonly IMapper _mapper;

    public QueueExportJobCommandHandlerTests()
    {
        MapperConfiguration mapperConfiguration = new(cfg =>
        {
            cfg.AddProfile<ExportJobMappingProfile>();
        });

        _mapper = mapperConfiguration.CreateMapper();
    }

    [Fact]
    public async Task Handle_Should_Create_ExportJob_And_Queue_Task()
    {
        InMemoryExportJobRepository exportJobRepository = new();
        InMemoryQueuedTaskRepository queuedTaskRepository = new();
        StubUnitOfWork unitOfWork = new();

        QueueExportJobCommandHandler handler = new(exportJobRepository, queuedTaskRepository, unitOfWork, _mapper);

        QueueExportJobCommand command = new(
            OwnerUserId: "operator@qphising.local",
            ExportType: ExportType.CampaignReport,
            Format: ExportFormat.Excel,
            CorrelationId: "corr-export-001");

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(ExportJobStatus.Queued, result.Value!.Status);
        Assert.Equal(1, exportJobRepository.Jobs.Count);
        Assert.Equal(1, queuedTaskRepository.Tasks.Count);

        QueuedTask queuedTask = queuedTaskRepository.Tasks.Single();
        Assert.Equal(TaskType.ExportGeneration, queuedTask.Type);
        Assert.Equal(result.Value.Id.ToString(), queuedTask.Payload.GetRequired("exportJobId"));
        Assert.Equal(command.ExportType.ToString(), queuedTask.Payload.GetRequired("exportType"));
        Assert.Equal(command.Format.ToString(), queuedTask.Payload.GetRequired("format"));
        Assert.Equal(command.OwnerUserId, queuedTask.Payload.GetRequired("requestedByUserId"));
        Assert.Equal(command.CorrelationId, queuedTask.CorrelationId);
        Assert.Equal(1, unitOfWork.SaveCallCount);
    }

    private sealed class InMemoryExportJobRepository : IExportJobRepository
    {
        public List<ExportJob> Jobs { get; } = [];

        public Task<ExportJob?> GetByIdAsync(Guid exportJobId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Jobs.SingleOrDefault(job => job.Id == exportJobId));
        }

        public Task<IReadOnlyCollection<ExportJob>> ListAsync(ExportJobReadCriteria criteria, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<ExportJob>>(Jobs);
        }

        public Task AddAsync(ExportJob exportJob, CancellationToken cancellationToken = default)
        {
            Jobs.Add(exportJob);
            return Task.CompletedTask;
        }

        public void Update(ExportJob exportJob)
        {
        }

        public Task<IReadOnlyCollection<ExportJob>> ListExpiredWithStoredFileAsync(
            DateTimeOffset asOfUtc,
            int take,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<ExportJob> expired = Jobs
                .Where(job => job.ExpiresAt.HasValue && job.ExpiresAt.Value <= asOfUtc && !string.IsNullOrWhiteSpace(job.StoragePath))
                .Take(take)
                .ToArray();

            return Task.FromResult(expired);
        }
    }

    private sealed class InMemoryQueuedTaskRepository : IQueuedTaskRepository
    {
        public List<QueuedTask> Tasks { get; } = [];

        public Task<QueuedTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Tasks.SingleOrDefault(task => task.Id == taskId));
        }

        public Task AddAsync(QueuedTask queuedTask, CancellationToken cancellationToken = default)
        {
            Tasks.Add(queuedTask);
            return Task.CompletedTask;
        }

        public void Update(QueuedTask queuedTask)
        {
        }

        public Task<int> RequeueExpiredClaimsAsync(DateTimeOffset? asOf = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(0);
        }

        public Task<QueuedTask?> ClaimNextAsync(TimeSpan leaseDuration, DateTimeOffset? asOf = null, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<QueuedTask?>(null);
        }
    }

    private sealed class StubUnitOfWork : IUnitOfWork
    {
        public int SaveCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCallCount++;
            return Task.FromResult(1);
        }
    }
}
