using Microsoft.EntityFrameworkCore;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Tasks;

namespace QPhising.Infrastructure.Persistence.Repositories;

public sealed class TaskExecutionLogRepository(QPhisingDbContext dbContext) : ITaskExecutionLogRepository
{
    public async Task AddAsync(TaskExecutionLog executionLog, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<TaskExecutionLog>().AddAsync(executionLog, cancellationToken);
    }

    public async Task<IReadOnlyCollection<TaskExecutionLog>> ListByTaskIdAsync(
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TaskExecutionLog>()
            .AsNoTracking()
            .Where(log => log.TaskId == taskId)
            .OrderBy(log => log.OccurredAt)
            .ToListAsync(cancellationToken);
    }
}
