using Microsoft.EntityFrameworkCore;
using QPhising.Domain.Campaigns;
using QPhising.Domain.Tracking;
using QPhising.Domain.Templates;
using QPhising.Infrastructure.Persistence.Configurations;

namespace QPhising.Infrastructure.Persistence;

public sealed class QPhisingDbContext(DbContextOptions<QPhisingDbContext> options) : DbContext(options)
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();

    public DbSet<Template> Templates => Set<Template>();

    public DbSet<TrackingClick> TrackingClicks => Set<TrackingClick>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CampaignEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TemplateEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TrackingClickEntityTypeConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
