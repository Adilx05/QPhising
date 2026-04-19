using Microsoft.EntityFrameworkCore;
using QPhising.Api.Infrastructure.Persistence.Entities;

namespace QPhising.Api.Infrastructure.Persistence;

public sealed class QPhisingDbContext : DbContext
{
    public QPhisingDbContext(DbContextOptions<QPhisingDbContext> options)
        : base(options)
    {
    }

    public DbSet<CampaignEntity> Campaigns => Set<CampaignEntity>();

    public DbSet<TemplateEntity> Templates => Set<TemplateEntity>();

    public DbSet<TrackingPageEntity> TrackingPages => Set<TrackingPageEntity>();

    public DbSet<VisitEventEntity> VisitEvents => Set<VisitEventEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        var campaign = modelBuilder.Entity<CampaignEntity>();
        campaign.ToTable("campaigns");
        campaign.HasKey(x => x.Id);
        campaign.Property(x => x.Id).HasColumnName("id");
        campaign.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        campaign.Property(x => x.TemplateId).HasColumnName("template_id").IsRequired();
        campaign.Property(x => x.LifecycleState).HasColumnName("lifecycle_state").IsRequired();
        campaign.Property(x => x.ScheduleStartsAtUtc).HasColumnName("schedule_starts_at_utc");
        campaign.Property(x => x.ScheduleEndsAtUtc).HasColumnName("schedule_ends_at_utc");
        campaign.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        campaign.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();

        var template = modelBuilder.Entity<TemplateEntity>();
        template.ToTable("templates");
        template.HasKey(x => x.Id);
        template.Property(x => x.Id).HasColumnName("id");
        template.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        template.Property(x => x.HtmlContent).HasColumnName("html_content").HasMaxLength(200000).IsRequired();
        template.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        template.Property(x => x.Tags).HasColumnName("tags").HasColumnType("jsonb").IsRequired();
        template.Property(x => x.LifecycleState).HasColumnName("lifecycle_state").IsRequired();
        template.Property(x => x.Version).HasColumnName("version").IsRequired();
        template.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        template.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();

        var trackingPage = modelBuilder.Entity<TrackingPageEntity>();
        trackingPage.ToTable("tracking_pages");
        trackingPage.HasKey(x => x.Id);
        trackingPage.Property(x => x.Id).HasColumnName("id");
        trackingPage.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(80).IsRequired();
        trackingPage.Property(x => x.Title).HasColumnName("title").HasMaxLength(160).IsRequired();
        trackingPage.Property(x => x.Description).HasColumnName("description").HasMaxLength(1000);
        trackingPage.Property(x => x.DestinationUrl).HasColumnName("destination_url").HasMaxLength(2048).IsRequired();
        trackingPage.Property(x => x.OwnerId).HasColumnName("owner_id").HasMaxLength(128).IsRequired();
        trackingPage.Property(x => x.TemplateId).HasColumnName("template_id");
        trackingPage.Property(x => x.PublishState).HasColumnName("publish_state").IsRequired();
        trackingPage.Property(x => x.RetentionDays).HasColumnName("retention_days");
        trackingPage.Property(x => x.MaskIpAddress).HasColumnName("mask_ip_address");
        trackingPage.Property(x => x.EnableBotFiltering).HasColumnName("enable_bot_filtering");
        trackingPage.Property(x => x.CaptureUtmParameters).HasColumnName("capture_utm_parameters");
        trackingPage.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        trackingPage.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        trackingPage.HasIndex(x => x.Slug).IsUnique();
        trackingPage.HasIndex(x => x.OwnerId);
        trackingPage.HasIndex(x => x.TemplateId);
        trackingPage.HasOne<TemplateEntity>()
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        var visitEvent = modelBuilder.Entity<VisitEventEntity>();
        visitEvent.ToTable("visit_events");
        visitEvent.HasKey(x => x.Id);
        visitEvent.Property(x => x.Id).HasColumnName("id");
        visitEvent.Property(x => x.TrackingPageId).HasColumnName("tracking_page_id").IsRequired();
        visitEvent.Property(x => x.OccurredAtUtc).HasColumnName("occurred_at_utc").IsRequired();
        visitEvent.Property(x => x.SessionId).HasColumnName("session_id").HasMaxLength(128).IsRequired();
        visitEvent.Property(x => x.VisitorFingerprint).HasColumnName("visitor_fingerprint").HasMaxLength(128).IsRequired();
        visitEvent.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(1024);
        visitEvent.Property(x => x.ReferrerUrl).HasColumnName("referrer_url").HasMaxLength(2048);
        visitEvent.Property(x => x.IpHash).HasColumnName("ip_hash").HasMaxLength(128);
        visitEvent.Property(x => x.IpAddressHashPolicy).HasColumnName("ip_address_hash_policy").IsRequired();
        visitEvent.HasIndex(x => new { x.TrackingPageId, x.OccurredAtUtc });
        visitEvent.HasIndex(x => new { x.TrackingPageId, x.SessionId, x.OccurredAtUtc });
        visitEvent.HasOne(x => x.TrackingPage)
            .WithMany(x => x.VisitEvents)
            .HasForeignKey(x => x.TrackingPageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
