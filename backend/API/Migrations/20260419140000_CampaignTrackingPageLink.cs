using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace QPhising.Api.Migrations
{
    public partial class CampaignTrackingPageLink : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "template_id",
                table: "campaigns",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "tracking_page_id",
                table: "campaigns",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE campaigns c
                SET tracking_page_id = source.id
                FROM (
                    SELECT tp.id, tp.template_id, ROW_NUMBER() OVER (PARTITION BY tp.template_id ORDER BY tp.created_at_utc ASC) AS rn
                    FROM tracking_pages tp
                    WHERE tp.template_id IS NOT NULL
                ) AS source
                WHERE c.template_id = source.template_id
                  AND source.rn = 1
                  AND c.tracking_page_id IS NULL;");

            migrationBuilder.Sql(@"
                UPDATE campaigns c
                SET tracking_page_id = source.id
                FROM (
                    SELECT tp.id, ROW_NUMBER() OVER (ORDER BY tp.created_at_utc ASC) AS rn
                    FROM tracking_pages tp
                ) AS source
                WHERE c.tracking_page_id IS NULL
                  AND source.rn = 1;");

            migrationBuilder.AlterColumn<Guid>(
                name: "tracking_page_id",
                table: "campaigns",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_tracking_page_id",
                table: "campaigns",
                column: "tracking_page_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_campaigns_tracking_pages_tracking_page_id",
                table: "campaigns",
                column: "tracking_page_id",
                principalTable: "tracking_pages",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_campaigns_tracking_pages_tracking_page_id",
                table: "campaigns");

            migrationBuilder.DropIndex(
                name: "IX_campaigns_tracking_page_id",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "tracking_page_id",
                table: "campaigns");

            migrationBuilder.AlterColumn<Guid>(
                name: "template_id",
                table: "campaigns",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
