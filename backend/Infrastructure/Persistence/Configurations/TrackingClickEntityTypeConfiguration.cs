using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPhising.Domain.Tracking;

namespace QPhising.Infrastructure.Persistence.Configurations;

public sealed class TrackingClickEntityTypeConfiguration : IEntityTypeConfiguration<TrackingClick>
{
    public void Configure(EntityTypeBuilder<TrackingClick> builder)
    {
        builder.ToTable("tracking_clicks");

        builder.HasKey(click => click.Id);
        builder.Property(click => click.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(click => click.CampaignId)
            .HasColumnName("campaign_id")
            .IsRequired();

        builder.Property(click => click.TrackingToken)
            .HasColumnName("tracking_token")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(click => click.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(click => click.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(click => click.Fingerprint)
            .HasColumnName("fingerprint")
            .HasMaxLength(256);

        builder.Property(click => click.ClickedAtUtc)
            .HasColumnName("clicked_at_utc")
            .IsRequired();

        builder.HasIndex(click => click.CampaignId)
            .HasDatabaseName("ix_tracking_clicks_campaign_id");

        builder.HasIndex(click => click.ClickedAtUtc)
            .HasDatabaseName("ix_tracking_clicks_clicked_at_utc");

        builder.HasIndex(click => new { click.CampaignId, click.ClickedAtUtc })
            .HasDatabaseName("ix_tracking_clicks_campaign_clicked_at");
    }
}
