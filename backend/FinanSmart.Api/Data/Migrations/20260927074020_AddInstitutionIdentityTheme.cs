using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionIdentityTheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "Institutions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HoverColor",
                table: "Institutions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "HoverColor",
                table: "Institutions");
        }
    }
}
