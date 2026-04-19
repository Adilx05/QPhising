using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPhising.Api.Migrations
{
    public partial class AddTrackingIpCapturePolicy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "capture_ip_address",
                table: "tracking_pages",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ip_address_hash_policy",
                table: "tracking_pages",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE tracking_pages
                SET capture_ip_address = COALESCE(NOT mask_ip_address, TRUE),
                    ip_address_hash_policy = CASE
                        WHEN mask_ip_address IS NULL THEN 2
                        WHEN mask_ip_address = TRUE THEN 2
                        ELSE 1
                    END;
            ");

            migrationBuilder.DropColumn(
                name: "mask_ip_address",
                table: "tracking_pages");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "mask_ip_address",
                table: "tracking_pages",
                type: "boolean",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE tracking_pages
                SET mask_ip_address = CASE
                        WHEN capture_ip_address IS DISTINCT FROM TRUE THEN TRUE
                        WHEN ip_address_hash_policy = 1 THEN FALSE
                        ELSE TRUE
                    END;
            ");

            migrationBuilder.DropColumn(
                name: "capture_ip_address",
                table: "tracking_pages");

            migrationBuilder.DropColumn(
                name: "ip_address_hash_policy",
                table: "tracking_pages");
        }
    }
}
