using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPhising.Domain.Exports;

namespace QPhising.Infrastructure.Persistence.Configurations;

public sealed class ExportJobEntityTypeConfiguration : IEntityTypeConfiguration<ExportJob>
{
    public void Configure(EntityTypeBuilder<ExportJob> builder)
    {
        builder.ToTable("export_jobs");

        builder.HasKey(job => job.Id);

        builder.Property(job => job.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(job => job.OwnerUserId)
            .HasColumnName("owner_user_id")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(job => job.ExportType)
            .HasColumnName("export_type")
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(job => job.Format)
            .HasColumnName("format")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(job => job.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(job => job.RequestedAt)
            .HasColumnName("requested_at")
            .IsRequired();

        builder.Property(job => job.QueuedAt).HasColumnName("queued_at");
        builder.Property(job => job.ProcessingStartedAt).HasColumnName("processing_started_at");
        builder.Property(job => job.CompletedAt).HasColumnName("completed_at");
        builder.Property(job => job.FailedAt).HasColumnName("failed_at");
        builder.Property(job => job.CanceledAt).HasColumnName("canceled_at");
        builder.Property(job => job.ExpiresAt).HasColumnName("expires_at");

        builder.Property(job => job.FileName)
            .HasColumnName("file_name")
            .HasMaxLength(255);

        builder.Property(job => job.StoragePath)
            .HasColumnName("storage_path")
            .HasMaxLength(1024);

        builder.Property(job => job.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(150);

        builder.Property(job => job.FileSizeBytes).HasColumnName("file_size_bytes");

        builder.Property(job => job.ErrorMessage)
            .HasColumnName("error_message")
            .HasMaxLength(2000);

        builder.Property(job => job.CorrelationId)
            .HasColumnName("correlation_id")
            .HasMaxLength(100);

        builder.HasIndex(job => job.OwnerUserId)
            .HasDatabaseName("ix_export_jobs_owner_user_id");

        builder.HasIndex(job => job.Status)
            .HasDatabaseName("ix_export_jobs_status");

        builder.HasIndex(job => new { job.OwnerUserId, job.Status, job.RequestedAt })
            .HasDatabaseName("ix_export_jobs_owner_status_requested_at");
    }
}
