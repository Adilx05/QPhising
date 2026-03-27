using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QPhising.Domain.Abstractions;
using QPhising.Domain.Tasks;
using QPhising.Domain.Tasks.Exceptions;

namespace QPhising.Infrastructure.Persistence.Repositories;

public sealed class QueuedTaskRepository(QPhisingDbContext dbContext) : IQueuedTaskRepository
{
    public async Task<QueuedTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await dbContext.QueuedTasks.SingleOrDefaultAsync(task => task.Id == taskId, cancellationToken);
    }

    public async Task AddAsync(QueuedTask queuedTask, CancellationToken cancellationToken = default)
    {
        await dbContext.QueuedTasks.AddAsync(queuedTask, cancellationToken);
    }

    public void Update(QueuedTask queuedTask)
    {
        dbContext.QueuedTasks.Update(queuedTask);
    }

    public async Task<int> RequeueExpiredClaimsAsync(DateTimeOffset? asOf = null, CancellationToken cancellationToken = default)
    {
        DateTimeOffset referenceTime = asOf ?? DateTimeOffset.UtcNow;

        return await dbContext.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE queued_tasks
            SET status = {TaskExecutionStatus.Queued.ToString()},
                claimed_at = NULL,
                lease_expires_at = NULL,
                started_at = NULL,
                next_attempt_at = {referenceTime}
            WHERE status = {TaskExecutionStatus.Claimed.ToString()}
              AND lease_expires_at IS NOT NULL
              AND lease_expires_at <= {referenceTime};
            """, cancellationToken);
    }

    public async Task<QueuedTask?> ClaimNextAsync(
        TimeSpan leaseDuration,
        DateTimeOffset? asOf = null,
        CancellationToken cancellationToken = default)
    {
        if (leaseDuration <= TimeSpan.Zero)
        {
            throw new TaskValidationException("Task lease duration must be greater than zero.");
        }

        DateTimeOffset claimedAt = asOf ?? DateTimeOffset.UtcNow;
        DateTimeOffset leaseExpiresAt = claimedAt.Add(leaseDuration);

        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        await RequeueExpiredClaimsAsync(claimedAt, cancellationToken);

        Guid? claimedTaskId = await ClaimNextQueuedTaskIdAsync(claimedAt, leaseExpiresAt, cancellationToken);
        if (!claimedTaskId.HasValue)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        QueuedTask claimedTask = await dbContext.QueuedTasks.SingleAsync(
            task => task.Id == claimedTaskId.Value,
            cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return claimedTask;
    }

    private async Task<Guid?> ClaimNextQueuedTaskIdAsync(
        DateTimeOffset claimedAt,
        DateTimeOffset leaseExpiresAt,
        CancellationToken cancellationToken)
    {
        DbConnection connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using DbCommand command = connection.CreateCommand();
        command.Transaction = dbContext.Database.CurrentTransaction?.GetDbTransaction();
        command.CommandText =
            """
            WITH candidate AS (
                SELECT id
                FROM queued_tasks
                WHERE status = @queued_status
                  AND next_attempt_at <= @as_of
                ORDER BY next_attempt_at, created_at, id
                FOR UPDATE SKIP LOCKED
                LIMIT 1
            )
            UPDATE queued_tasks AS task
            SET status = @claimed_status,
                claimed_at = @claimed_at,
                lease_expires_at = @lease_expires_at
            FROM candidate
            WHERE task.id = candidate.id
            RETURNING task.id;
            """;

        command.Parameters.Add(CreateParameter(command, "@queued_status", TaskExecutionStatus.Queued.ToString()));
        command.Parameters.Add(CreateParameter(command, "@as_of", claimedAt));
        command.Parameters.Add(CreateParameter(command, "@claimed_status", TaskExecutionStatus.Claimed.ToString()));
        command.Parameters.Add(CreateParameter(command, "@claimed_at", claimedAt));
        command.Parameters.Add(CreateParameter(command, "@lease_expires_at", leaseExpiresAt));

        object? result = await command.ExecuteScalarAsync(cancellationToken);
        return result is Guid id ? id : null;
    }

    private static DbParameter CreateParameter(DbCommand command, string name, object value)
    {
        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        return parameter;
    }
}
