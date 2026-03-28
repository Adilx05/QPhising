using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPhising.Domain.Tasks;

namespace QPhising.Infrastructure.Persistence.Configurations;

public sealed class QueuedTaskEntityTypeConfiguration : IEntityTypeConfiguration<QueuedTask>
{
    public void Configure(EntityTypeBuilder<QueuedTask> builder)
    {
        builder.ToTable("queued_tasks");

        builder.HasKey(task => task.Id);
        builder.Property(task => task.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(task => task.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(task => task.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(task => task.Payload)
            .HasColumnName("payload_json")
            .HasColumnType("jsonb")
            .HasConversion(
                payload => JsonSerializer.Serialize(payload.Values, (JsonSerializerOptions?)null),
                serializedPayload => DeserializePayload(serializedPayload))
            .IsRequired();

        builder.Property(task => task.AttemptCount)
            .HasColumnName("attempt_count")
            .IsRequired();

        builder.Property(task => task.MaxAttempts)
            .HasColumnName("max_attempts")
            .IsRequired();

        builder.Property(task => task.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(task => task.ClaimedAt)
            .HasColumnName("claimed_at");

        builder.Property(task => task.StartedAt)
            .HasColumnName("started_at");

        builder.Property(task => task.CompletedAt)
            .HasColumnName("completed_at");

        builder.Property(task => task.LastFailedAt)
            .HasColumnName("last_failed_at");

        builder.Property(task => task.LeaseExpiresAt)
            .HasColumnName("lease_expires_at");

        builder.Property(task => task.NextAttemptAt)
            .HasColumnName("next_attempt_at")
            .IsRequired();

        builder.Property(task => task.LastError)
            .HasColumnName("last_error")
            .HasMaxLength(2048);

        builder.Property(task => task.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(128);

        builder.HasIndex(task => new { task.Status, task.CreatedAt })
            .HasDatabaseName("ix_queued_tasks_status_created_at");

        builder.HasIndex(task => task.LeaseExpiresAt)
            .HasDatabaseName("ix_queued_tasks_lease_expires_at");

        builder.HasIndex(task => new { task.Status, task.NextAttemptAt, task.CreatedAt })
            .HasDatabaseName("ix_queued_tasks_status_next_attempt_at_created_at");
    }

    private static TaskPayload DeserializePayload(string serializedPayload)
    {
        Dictionary<string, string>? payloadValues =
            JsonSerializer.Deserialize<Dictionary<string, string>>(serializedPayload, (JsonSerializerOptions?)null);

        if (payloadValues is null)
        {
            throw new InvalidOperationException("Queued task payload cannot be null.");
        }

        return TaskPayload.Create(payloadValues);
    }
}
