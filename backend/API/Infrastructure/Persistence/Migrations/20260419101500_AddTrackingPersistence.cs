using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPhising.Api.Infrastructure.Persistence.Migrations;

public partial class AddTrackingPersistence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "tracking_pages",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                destination_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                owner_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                publish_state = table.Column<int>(type: "integer", nullable: false),
                retention_days = table.Column<int>(type: "integer", nullable: true),
                mask_ip_address = table.Column<bool>(type: "boolean", nullable: true),
                enable_bot_filtering = table.Column<bool>(type: "boolean", nullable: true),
                capture_utm_parameters = table.Column<bool>(type: "boolean", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_tracking_pages", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "visit_events",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tracking_page_id = table.Column<Guid>(type: "uuid", nullable: false),
                occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                session_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                visitor_fingerprint = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                user_agent = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                referrer_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                ip_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                ip_address_hash_policy = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_visit_events", x => x.id);
                table.ForeignKey(
                    name: "FK_visit_events_tracking_pages_tracking_page_id",
                    column: x => x.tracking_page_id,
                    principalTable: "tracking_pages",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_tracking_pages_owner_id",
            table: "tracking_pages",
            column: "owner_id");

        migrationBuilder.CreateIndex(
            name: "IX_tracking_pages_slug",
            table: "tracking_pages",
            column: "slug",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_visit_events_tracking_page_id_occurred_at_utc",
            table: "visit_events",
            columns: new[] { "tracking_page_id", "occurred_at_utc" });

        migrationBuilder.CreateIndex(
            name: "IX_visit_events_tracking_page_id_session_id_occurred_at_utc",
            table: "visit_events",
            columns: new[] { "tracking_page_id", "session_id", "occurred_at_utc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "visit_events");

        migrationBuilder.DropTable(
            name: "tracking_pages");
    }
}
