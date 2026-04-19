using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPhising.Api.Migrations
{
    public partial class TrackingPagePublicValidityAndHtml : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "destination_url",
                table: "tracking_pages");

            migrationBuilder.AddColumn<string>(
                name: "custom_html_content",
                table: "tracking_pages",
                type: "character varying(200000)",
                maxLength: 200000,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "valid_from_utc",
                table: "tracking_pages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "valid_until_utc",
                table: "tracking_pages",
                type: "timestamp with time zone",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "custom_html_content",
                table: "tracking_pages");

            migrationBuilder.DropColumn(
                name: "valid_from_utc",
                table: "tracking_pages");

            migrationBuilder.DropColumn(
                name: "valid_until_utc",
                table: "tracking_pages");

            migrationBuilder.AddColumn<string>(
                name: "destination_url",
                table: "tracking_pages",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "https://example.com");
        }
    }
}
