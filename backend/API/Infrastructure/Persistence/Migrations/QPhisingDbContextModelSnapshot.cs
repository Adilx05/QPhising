using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

                b.Property<string>("HtmlContent")
                    .IsRequired()
                    .HasMaxLength(200000)
                    .HasColumnType("character varying(200000)")
                    .HasColumnName("html_content");

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

        modelBuilder.Entity("QPhising.Api.Infrastructure.Persistence.Entities.TrackingPageEntity", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<bool?>("CaptureUtmParameters")
                    .HasColumnType("boolean")
                    .HasColumnName("capture_utm_parameters");

                b.Property<DateTimeOffset>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at_utc");

                b.Property<string>("Description")
                    .HasMaxLength(1000)
                    .HasColumnType("character varying(1000)")
                    .HasColumnName("description");

                b.Property<string>("DestinationUrl")
                    .IsRequired()
                    .HasMaxLength(2048)
                    .HasColumnType("character varying(2048)")
                    .HasColumnName("destination_url");

                b.Property<bool?>("EnableBotFiltering")
                    .HasColumnType("boolean")
                    .HasColumnName("enable_bot_filtering");

                b.Property<bool?>("MaskIpAddress")
                    .HasColumnType("boolean")
                    .HasColumnName("mask_ip_address");

                b.Property<string>("OwnerId")
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnType("character varying(128)")
                    .HasColumnName("owner_id");

                b.Property<Guid?>("TemplateId")
                    .HasColumnType("uuid")
                    .HasColumnName("template_id");

                b.Property<int>("PublishState")
                    .HasColumnType("integer")
                    .HasColumnName("publish_state");

                b.Property<int?>("RetentionDays")
                    .HasColumnType("integer")
                    .HasColumnName("retention_days");

                b.Property<string>("Slug")
                    .IsRequired()
                    .HasMaxLength(80)
                    .HasColumnType("character varying(80)")
                    .HasColumnName("slug");

                b.Property<string>("Title")
                    .IsRequired()
                    .HasMaxLength(160)
                    .HasColumnType("character varying(160)")
                    .HasColumnName("title");

                b.Property<DateTimeOffset>("UpdatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("updated_at_utc");

                b.HasKey("Id");

                b.HasIndex("OwnerId")
                    .HasDatabaseName("IX_tracking_pages_owner_id");

                b.HasIndex("Slug")
                    .IsUnique()
                    .HasDatabaseName("IX_tracking_pages_slug");

                b.HasIndex("TemplateId")
                    .HasDatabaseName("IX_tracking_pages_template_id");

                b.ToTable("tracking_pages", (string)null);
            });

        modelBuilder.Entity("QPhising.Api.Infrastructure.Persistence.Entities.VisitEventEntity", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<int>("IpAddressHashPolicy")
                    .HasColumnType("integer")
                    .HasColumnName("ip_address_hash_policy");

                b.Property<string>("IpHash")
                    .HasMaxLength(128)
                    .HasColumnType("character varying(128)")
                    .HasColumnName("ip_hash");

                b.Property<DateTimeOffset>("OccurredAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("occurred_at_utc");

                b.Property<string>("ReferrerUrl")
                    .HasMaxLength(2048)
                    .HasColumnType("character varying(2048)")
                    .HasColumnName("referrer_url");

                b.Property<string>("SessionId")
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnType("character varying(128)")
                    .HasColumnName("session_id");

                b.Property<Guid>("TrackingPageId")
                    .HasColumnType("uuid")
                    .HasColumnName("tracking_page_id");

                b.Property<string>("UserAgent")
                    .HasMaxLength(1024)
                    .HasColumnType("character varying(1024)")
                    .HasColumnName("user_agent");

                b.Property<string>("VisitorFingerprint")
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnType("character varying(128)")
                    .HasColumnName("visitor_fingerprint");

                b.HasKey("Id");

                b.HasIndex("TrackingPageId", "OccurredAtUtc")
                    .HasDatabaseName("IX_visit_events_tracking_page_id_occurred_at_utc");

                b.HasIndex("TrackingPageId", "SessionId", "OccurredAtUtc")
                    .HasDatabaseName("IX_visit_events_tracking_page_id_session_id_occurred_at_utc");

                b.ToTable("visit_events", (string)null);
            });

        modelBuilder.Entity("QPhising.Api.Infrastructure.Persistence.Entities.TrackingPageEntity", b =>
            {
                b.HasOne("QPhising.Api.Infrastructure.Persistence.Entities.TemplateEntity", null)
                    .WithMany()
                    .HasForeignKey("TemplateId")
                    .OnDelete(DeleteBehavior.SetNull);
            });

        modelBuilder.Entity("QPhising.Api.Infrastructure.Persistence.Entities.VisitEventEntity", b =>
            {
                b.HasOne("QPhising.Api.Infrastructure.Persistence.Entities.TrackingPageEntity", "TrackingPage")
                    .WithMany("VisitEvents")
                    .HasForeignKey("TrackingPageId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("TrackingPage");
            });

        modelBuilder.Entity("QPhising.Api.Infrastructure.Persistence.Entities.TrackingPageEntity", b =>
            {
                b.Navigation("VisitEvents");
            });
#pragma warning restore 612, 618
    }
}
