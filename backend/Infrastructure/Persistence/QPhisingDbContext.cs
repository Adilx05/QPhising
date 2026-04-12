using Microsoft.EntityFrameworkCore;
using QPhising.Domain.Campaigns;
using QPhising.Domain.Exports;
using QPhising.Domain.Tasks;
using QPhising.Domain.Tracking;
using QPhising.Domain.Templates;
using QPhising.Domain.Setup;
using QPhising.Domain.Configuration;
using QPhising.Infrastructure.Persistence.Configurations;

namespace QPhising.Infrastructure.Persistence;

public sealed class QPhisingDbContext(DbContextOptions<QPhisingDbContext> options) : DbContext(options)
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();

    public DbSet<Template> Templates => Set<Template>();

    public DbSet<TrackingClick> TrackingClicks => Set<TrackingClick>();

    public DbSet<QueuedTask> QueuedTasks => Set<QueuedTask>();

    public DbSet<TaskExecutionLog> TaskExecutionLogs => Set<TaskExecutionLog>();

    public DbSet<ExportJob> ExportJobs => Set<ExportJob>();

    public DbSet<SetupState> SetupStates => Set<SetupState>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CampaignEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TemplateEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TrackingClickEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new QueuedTaskEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TaskExecutionLogEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new ExportJobEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SetupStateEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new SystemSettingEntityTypeConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
