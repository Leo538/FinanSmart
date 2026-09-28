using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestmentRateSourceMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "SourceDate",
                table: "InvestmentRates",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceName",
                table: "InvestmentRates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "InvestmentRates",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "InvestmentRates"
                SET "SourceName" = 'Tarifario Banco Pichincha, septiembre 2026',
                    "SourceUrl" = 'https://www.pichincha.com/sites/default/files/documents/2026-09/tarifario-web-septiembre-2026-3.pdf',
                    "SourceDate" = DATE '2026-09-01'
                WHERE "InvestmentProductId" = 'd3000000-0000-0000-0000-000000000001';

                UPDATE "InvestmentRates"
                SET "SourceName" = 'Tarifario Banco del Pacífico, septiembre 2026',
                    "SourceUrl" = 'https://www.bancodelpacifico.com/BancoPacifico/media/pdf/TranspInformacion/2026/Tasas-Pasivas.pdf',
                    "SourceDate" = DATE '2026-09-01'
                WHERE "InvestmentProductId" = 'd3000000-0000-0000-0000-000000000002';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceDate",
                table: "InvestmentRates");

            migrationBuilder.DropColumn(
                name: "SourceName",
                table: "InvestmentRates");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "InvestmentRates");
        }
    }
}
