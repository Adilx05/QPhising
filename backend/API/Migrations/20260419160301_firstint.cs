using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPhising.Api.Migrations
{
    /// <inheritdoc />
    public partial class firstint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    html_content = table.Column<string>(type: "character varying(200000)", maxLength: 200000, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    tags = table.Column<string>(type: "jsonb", nullable: false),
                    lifecycle_state = table.Column<int>(type: "integer", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tracking_pages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    title = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    owner_id = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    custom_html_content = table.Column<string>(type: "character varying(200000)", maxLength: 200000, nullable: true),
                    valid_from_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    valid_until_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    publish_state = table.Column<int>(type: "integer", nullable: false),
                    retention_days = table.Column<int>(type: "integer", nullable: true),
                    capture_ip_address = table.Column<bool>(type: "boolean", nullable: true),
                    ip_address_hash_policy = table.Column<int>(type: "integer", nullable: true),
                    enable_bot_filtering = table.Column<bool>(type: "boolean", nullable: true),
                    capture_utm_parameters = table.Column<bool>(type: "boolean", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tracking_pages", x => x.id);
                    table.ForeignKey(
                        name: "FK_tracking_pages_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "campaigns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    tracking_page_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: true),
                    lifecycle_state = table.Column<int>(type: "integer", nullable: false),
                    schedule_starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    schedule_ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_campaigns", x => x.id);
                    table.ForeignKey(
                        name: "FK_campaigns_tracking_pages_tracking_page_id",
                        column: x => x.tracking_page_id,
                        principalTable: "tracking_pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_campaigns_tracking_page_id",
                table: "campaigns",
                column: "tracking_page_id",
                unique: true,
                filter: "\"is_deleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_pages_owner_id",
                table: "tracking_pages",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_pages_slug",
                table: "tracking_pages",
                column: "slug",
                unique: true,
                filter: "\"is_deleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_pages_template_id",
                table: "tracking_pages",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_visit_events_tracking_page_id_occurred_at_utc",
                table: "visit_events",
                columns: new[] { "tracking_page_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_visit_events_tracking_page_id_session_id_occurred_at_utc",
                table: "visit_events",
                columns: new[] { "tracking_page_id", "session_id", "occurred_at_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "campaigns");

            migrationBuilder.DropTable(
                name: "visit_events");

            migrationBuilder.DropTable(
                name: "tracking_pages");

            migrationBuilder.DropTable(
                name: "templates");
        }
    }
}
