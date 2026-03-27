using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPhising.Domain.Tasks;

namespace QPhising.Infrastructure.Persistence.Configurations;

public sealed class TaskExecutionLogEntityTypeConfiguration : IEntityTypeConfiguration<TaskExecutionLog>
{
    public void Configure(EntityTypeBuilder<TaskExecutionLog> builder)
    {
        builder.ToTable("task_execution_logs");

        builder.HasKey(log => log.Id);
        builder.Property(log => log.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(log => log.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(log => log.EventType)
            .HasColumnName("event_type")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(log => log.TaskStatus)
            .HasColumnName("task_status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(log => log.AttemptNumber)
            .HasColumnName("attempt_number")
            .IsRequired();

        builder.Property(log => log.OccurredAt)
            .HasColumnName("occurred_at")
            .IsRequired();

        builder.Property(log => log.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(128);

        builder.Property(log => log.Details)
            .HasColumnName("details")
            .HasMaxLength(4000);

        builder.Property(log => log.ExecutionDurationMilliseconds)
            .HasColumnName("execution_duration_ms");

        builder.HasIndex(log => new { log.TaskId, log.OccurredAt })
            .HasDatabaseName("ix_task_execution_logs_task_id_occurred_at");

        builder.HasIndex(log => log.CorrelationId)
            .HasDatabaseName("ix_task_execution_logs_correlation_id");

        builder.HasOne<QueuedTask>()
            .WithMany()
            .HasForeignKey(log => log.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
