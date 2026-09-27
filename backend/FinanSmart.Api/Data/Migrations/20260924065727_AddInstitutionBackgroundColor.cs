using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInstitutionBackgroundColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                table: "Institutions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                table: "Institutions");
        }
    }
}
