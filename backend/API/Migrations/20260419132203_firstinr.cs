using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPhising.Api.Migrations
{
    /// <inheritdoc />
    public partial class firstinr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "mask_ip_address",
                table: "tracking_pages",
                newName: "capture_ip_address");

            migrationBuilder.AddColumn<int>(
                name: "ip_address_hash_policy",
                table: "tracking_pages",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ip_address_hash_policy",
                table: "tracking_pages");

            migrationBuilder.RenameColumn(
                name: "capture_ip_address",
                table: "tracking_pages",
                newName: "mask_ip_address");
        }
    }
}
