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

    public DbSet<CampaignTargetEntity> CampaignTargets => Set<CampaignTargetEntity>();

    public DbSet<TemplateEntity> Templates => Set<TemplateEntity>();

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

        campaign.HasMany(x => x.Targets)
            .WithOne()
            .HasForeignKey(x => x.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        var campaignTarget = modelBuilder.Entity<CampaignTargetEntity>();
        campaignTarget.ToTable("campaign_targets");
        campaignTarget.HasKey(x => x.Id);
        campaignTarget.Property(x => x.Id).HasColumnName("id");
        campaignTarget.Property(x => x.CampaignId).HasColumnName("campaign_id").IsRequired();
        campaignTarget.Property(x => x.EmailAddress).HasColumnName("email_address").HasMaxLength(320).IsRequired();

        campaignTarget.HasIndex(x => new { x.CampaignId, x.EmailAddress })
            .IsUnique();


        var template = modelBuilder.Entity<TemplateEntity>();
        template.ToTable("templates");
        template.HasKey(x => x.Id);
        template.Property(x => x.Id).HasColumnName("id");
        template.Property(x => x.Name).HasColumnName("name").HasMaxLength(120).IsRequired();
        template.Property(x => x.Subject).HasColumnName("subject").HasMaxLength(180).IsRequired();
        template.Property(x => x.Body).HasColumnName("body").HasMaxLength(200000).IsRequired();
        template.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
        template.Property(x => x.Tags).HasColumnName("tags").HasColumnType("jsonb").IsRequired();
        template.Property(x => x.LifecycleState).HasColumnName("lifecycle_state").IsRequired();
        template.Property(x => x.Version).HasColumnName("version").IsRequired();
        template.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        template.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
    }
}
