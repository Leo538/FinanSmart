using FinanSmart.Api.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations;

[DbContext(typeof(FinanSmartDbContext))]
[Migration("20260928003000_AddCreditRateSourceAndOptionalEffectiveDates")]
public partial class AddCreditRateSourceAndOptionalEffectiveDates : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTimeOffset>(
            name: "EffectiveFrom",
            table: "CreditRates",
            type: "timestamp with time zone",
            nullable: true,
            oldClrType: typeof(DateTimeOffset),
            oldType: "timestamp with time zone");

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "SourceDate",
            table: "CreditRates",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SourceUrl",
            table: "CreditRates",
            type: "character varying(2048)",
            maxLength: 2048,
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE "CreditRates"
            SET "EffectiveFrom" = NULL,
                "SourceDate" = '2026-09-28T00:00:00Z',
                "SourceUrl" = 'https://www.bancodelpacifico.com/personas/creditos/otros-creditos/credito-agil'
            WHERE "Id" = 'b1000000-0000-0000-0000-000000000001';

            UPDATE "CreditRates"
            SET "EffectiveFrom" = NULL,
                "SourceDate" = '2026-09-28T00:00:00Z',
                "SourceUrl" = 'https://www.bancodelpacifico.com/personas/creditos/otros-creditos/credito-respaldado-por-tu-inversion'
            WHERE "Id" = 'b1000000-0000-0000-0000-000000000002';

            UPDATE "CreditRates"
            SET "EffectiveFrom" = NULL,
                "SourceDate" = '2026-09-28T00:00:00Z',
                "SourceUrl" = 'https://landing.bancoguayaquil.com/creditos/multicredito/'
            WHERE "Id" = 'b1000000-0000-0000-0000-000000000003';

            UPDATE "CreditRates"
            SET "SourceDate" = "EffectiveFrom"
            WHERE "Id" IN (
                'b1000000-0000-0000-0000-000000000005',
                'b1000000-0000-0000-0000-000000000006',
                'b1000000-0000-0000-0000-000000000007',
                'b1000000-0000-0000-0000-000000000008'
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        throw new NotSupportedException(
            "No se puede revertir esta migración sin inventar fechas de vigencia para tasas cuya fuente no las publica.");
    }
}
