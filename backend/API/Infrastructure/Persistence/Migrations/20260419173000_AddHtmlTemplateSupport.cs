using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QPhising.Api.Infrastructure.Persistence.Migrations;

public partial class AddHtmlTemplateSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "body",
            table: "templates",
            newName: "html_content");

        migrationBuilder.DropColumn(
            name: "subject",
            table: "templates");

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
            defaultValue: "Template Subject");
    }
}
