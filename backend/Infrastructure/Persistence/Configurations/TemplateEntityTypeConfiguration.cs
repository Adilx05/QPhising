using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QPhising.Domain.Templates;

namespace QPhising.Infrastructure.Persistence.Configurations;

public sealed class TemplateEntityTypeConfiguration : IEntityTypeConfiguration<Template>
{
    public void Configure(EntityTypeBuilder<Template> builder)
    {
        builder.ToTable("templates");

        builder.HasKey(template => template.Id);
        builder.Property(template => template.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(template => template.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(template => template.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(template => template.HtmlContent)
            .HasColumnName("html_content")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(template => template.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(template => template.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        builder.OwnsMany<TemplateVariable>("_variables", variablesBuilder =>
        {
            variablesBuilder.ToTable("template_variables");

            variablesBuilder.WithOwner()
                .HasForeignKey("template_id");

            variablesBuilder.Property<Guid>("id")
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            variablesBuilder.HasKey("id");

            variablesBuilder.Property(variable => variable.Name)
                .HasColumnName("name")
                .HasMaxLength(64)
                .IsRequired();

            variablesBuilder.HasIndex("template_id", nameof(TemplateVariable.Name))
                .IsUnique()
                .HasDatabaseName("ux_template_variables_template_name");
        });

        builder.Navigation(template => template.Variables)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(template => template.Name)
            .HasDatabaseName("ix_templates_name");

        builder.HasIndex(template => template.Status)
            .HasDatabaseName("ix_templates_status");

        builder.HasIndex(template => template.Type)
            .HasDatabaseName("ix_templates_type");

        builder.HasIndex(template => new { template.Status, template.Type })
            .HasDatabaseName("ix_templates_status_type");

        builder.HasIndex(template => template.Name)
            .HasFilter("status = 'Published'")
            .IsUnique()
            .HasDatabaseName("ux_templates_published_name");
    }
}
