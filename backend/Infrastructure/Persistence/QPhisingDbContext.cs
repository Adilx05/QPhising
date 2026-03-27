using Microsoft.EntityFrameworkCore;
using QPhising.Domain.Campaigns;
using QPhising.Infrastructure.Persistence.Configurations;

namespace QPhising.Infrastructure.Persistence;

public sealed class QPhisingDbContext(DbContextOptions<QPhisingDbContext> options) : DbContext(options)
{
    public DbSet<Campaign> Campaigns => Set<Campaign>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CampaignEntityTypeConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
