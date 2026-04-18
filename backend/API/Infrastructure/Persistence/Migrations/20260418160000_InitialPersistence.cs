using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPhising.Api.Infrastructure.Persistence.Migrations;

public partial class InitialPersistence : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "campaigns",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                template_id = table.Column<Guid>(type: "uuid", nullable: false),
                lifecycle_state = table.Column<int>(type: "integer", nullable: false),
                schedule_starts_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                schedule_ends_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_campaigns", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "campaign_targets",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                campaign_id = table.Column<Guid>(type: "uuid", nullable: false),
                email_address = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_campaign_targets", x => x.id);
                table.ForeignKey(
                    name: "FK_campaign_targets_campaigns_campaign_id",
                    column: x => x.campaign_id,
                    principalTable: "campaigns",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_campaign_targets_campaign_id_email_address",
            table: "campaign_targets",
            columns: new[] { "campaign_id", "email_address" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "campaign_targets");

        migrationBuilder.DropTable(
            name: "campaigns");
    }
}
