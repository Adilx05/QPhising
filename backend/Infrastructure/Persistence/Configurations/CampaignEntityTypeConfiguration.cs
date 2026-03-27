using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPhising.Domain.Campaigns;

namespace QPhising.Infrastructure.Persistence.Configurations;

public sealed class CampaignEntityTypeConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("campaigns");

        builder.HasKey(campaign => campaign.Id);
        builder.Property(campaign => campaign.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(campaign => campaign.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(campaign => campaign.TemplateType)
            .HasColumnName("template_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(campaign => campaign.HtmlContent)
            .HasColumnName("html_content")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(campaign => campaign.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(campaign => campaign.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(campaign => campaign.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Ignore(campaign => campaign.StatusEvents);

        builder.HasIndex(campaign => campaign.Status)
            .HasDatabaseName("ix_campaigns_status");

        builder.HasIndex(campaign => campaign.StartDate)
            .HasDatabaseName("ix_campaigns_start_date");

        builder.HasIndex(campaign => campaign.EndDate)
            .HasDatabaseName("ix_campaigns_end_date");

        builder.HasIndex(campaign => new { campaign.Status, campaign.StartDate, campaign.EndDate })
            .HasDatabaseName("ix_campaigns_status_start_end");
    }
}
