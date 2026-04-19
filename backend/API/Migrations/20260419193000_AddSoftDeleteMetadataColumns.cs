using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPhising.Api.Migrations
{
    public partial class AddSoftDeleteMetadataColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at_utc",
                table: "tracking_pages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                table: "tracking_pages",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at_utc",
                table: "templates",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                table: "templates",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at_utc",
                table: "campaigns",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                table: "campaigns",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at_utc",
                table: "tracking_pages");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "tracking_pages");

            migrationBuilder.DropColumn(
                name: "deleted_at_utc",
                table: "templates");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "templates");

            migrationBuilder.DropColumn(
                name: "deleted_at_utc",
                table: "campaigns");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "campaigns");
        }
    }
}
