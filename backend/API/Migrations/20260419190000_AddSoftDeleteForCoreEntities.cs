using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPhising.Api.Migrations
{
    public partial class AddSoftDeleteForCoreEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "campaigns",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "tracking_pages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.DropIndex(
                name: "IX_tracking_pages_slug",
                table: "tracking_pages");

            migrationBuilder.DropIndex(
                name: "IX_campaigns_tracking_page_id",
                table: "campaigns");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_pages_slug",
                table: "tracking_pages",
                column: "slug",
                unique: true,
                filter: "\"is_deleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_tracking_page_id",
                table: "campaigns",
                column: "tracking_page_id",
                unique: true,
                filter: "\"is_deleted\" = FALSE");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tracking_pages_slug",
                table: "tracking_pages");

            migrationBuilder.DropIndex(
                name: "IX_campaigns_tracking_page_id",
                table: "campaigns");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_pages_slug",
                table: "tracking_pages",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_campaigns_tracking_page_id",
                table: "campaigns",
                column: "tracking_page_id",
                unique: true);

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "templates");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "tracking_pages");
        }
    }
}
