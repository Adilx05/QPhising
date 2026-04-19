using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using QPhising.Api.Infrastructure.Persistence;

#nullable disable

namespace QPhising.Api.Infrastructure.Persistence.Migrations;

[DbContext(typeof(QPhisingDbContext))]
partial class QPhisingDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.4")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("QPhising.Api.Infrastructure.Persistence.Entities.CampaignEntity", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<DateTimeOffset>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at_utc");

                b.Property<int>("LifecycleState")
                    .HasColumnType("integer")
                    .HasColumnName("lifecycle_state");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(120)
                    .HasColumnType("character varying(120)")
                    .HasColumnName("name");

                b.Property<DateTimeOffset?>("ScheduleEndsAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("schedule_ends_at_utc");

                b.Property<DateTimeOffset?>("ScheduleStartsAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("schedule_starts_at_utc");

                b.Property<Guid>("TemplateId")
                    .HasColumnType("uuid")
                    .HasColumnName("template_id");

                b.Property<DateTimeOffset>("UpdatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("updated_at_utc");

                b.HasKey("Id");

                b.ToTable("campaigns", (string)null);
            });



        modelBuilder.Entity("QPhising.Api.Infrastructure.Persistence.Entities.TemplateEntity", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<string>("Body")
                    .IsRequired()
                    .HasMaxLength(200000)
                    .HasColumnType("character varying(200000)")
                    .HasColumnName("body");

                b.Property<DateTimeOffset>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at_utc");

                b.Property<string>("Description")
                    .HasMaxLength(500)
                    .HasColumnType("character varying(500)")
                    .HasColumnName("description");

                b.Property<int>("LifecycleState")
                    .HasColumnType("integer")
                    .HasColumnName("lifecycle_state");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(120)
                    .HasColumnType("character varying(120)")
                    .HasColumnName("name");

                b.Property<string>("Subject")
                    .IsRequired()
                    .HasMaxLength(180)
                    .HasColumnType("character varying(180)")
                    .HasColumnName("subject");

                b.Property<string>("Tags")
                    .IsRequired()
                    .HasColumnType("jsonb")
                    .HasColumnName("tags");

                b.Property<DateTimeOffset>("UpdatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("updated_at_utc");

                b.Property<int>("Version")
                    .HasColumnType("integer")
                    .HasColumnName("version");

                b.HasKey("Id");

                b.ToTable("templates", (string)null);
            });

#pragma warning restore 612, 618
    }
}
