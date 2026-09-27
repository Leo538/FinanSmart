using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UsePublicLandingInstitutionPalette : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Institutions"
                SET "PrimaryColor" = '#164A5C',
                    "SecondaryColor" = '#00666D',
                    "BackgroundColor" = '#F7F9FA',
                    "HoverColor" = '#0D3747',
                    "UpdatedAt" = NOW()
                WHERE "IsActive" = TRUE
                  AND LOWER("PrimaryColor") = '#0055ff'
                  AND LOWER("SecondaryColor") = '#ffffff'
                  AND LOWER("BackgroundColor") = '#ffffff';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Institutions"
                SET "PrimaryColor" = '#0055ff',
                    "SecondaryColor" = '#ffffff',
                    "BackgroundColor" = '#ffffff',
                    "HoverColor" = NULL,
                    "UpdatedAt" = NOW()
                WHERE "IsActive" = TRUE
                  AND LOWER("PrimaryColor") = '#164a5c'
                  AND LOWER("SecondaryColor") = '#00666d'
                  AND LOWER("BackgroundColor") = '#f7f9fa'
                  AND LOWER("HoverColor") = '#0d3747';
                """);
        }
    }
}
