using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPhising.Api.Migrations
{
    /// <inheritdoc />
    public partial class firstina : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "subject",
                table: "templates");

            migrationBuilder.RenameColumn(
                name: "body",
                table: "templates",
                newName: "html_content");

            migrationBuilder.AddColumn<Guid>(
                name: "template_id",
                table: "tracking_pages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tracking_pages_template_id",
                table: "tracking_pages",
                column: "template_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tracking_pages_templates_template_id",
                table: "tracking_pages",
                column: "template_id",
                principalTable: "templates",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tracking_pages_templates_template_id",
                table: "tracking_pages");

            migrationBuilder.DropIndex(
                name: "IX_tracking_pages_template_id",
                table: "tracking_pages");

            migrationBuilder.DropColumn(
                name: "template_id",
                table: "tracking_pages");

            migrationBuilder.RenameColumn(
                name: "html_content",
                table: "templates",
                newName: "body");

            migrationBuilder.AddColumn<string>(
                name: "subject",
                table: "templates",
                type: "character varying(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");
        }
    }
}
