using FinanSmart.Api.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations;

[DbContext(typeof(FinanSmartDbContext))]
[Migration("20260928000000_MakeCreditTypeLimitsOptional")]
public partial class MakeCreditTypeLimitsOptional : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "MinimumAmount",
            table: "CreditTypes",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2);

        migrationBuilder.AlterColumn<decimal>(
            name: "MaximumAmount",
            table: "CreditTypes",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2);

        migrationBuilder.AlterColumn<int>(
            name: "MinimumTermMonths",
            table: "CreditTypes",
            type: "integer",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer");

        migrationBuilder.AlterColumn<int>(
            name: "MaximumTermMonths",
            table: "CreditTypes",
            type: "integer",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "integer");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT 1 FROM "CreditTypes"
                    WHERE "MinimumAmount" IS NULL OR "MaximumAmount" IS NULL
                       OR "MinimumTermMonths" IS NULL OR "MaximumTermMonths" IS NULL
                ) THEN
                    RAISE EXCEPTION 'Cannot restore required credit limits while CreditTypes contains unspecified limits.';
                END IF;
            END $$;
            """);

        migrationBuilder.AlterColumn<decimal>(name: "MinimumAmount", table: "CreditTypes", type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, oldClrType: typeof(decimal), oldType: "numeric(18,2)", oldPrecision: 18, oldScale: 2, oldNullable: true);
        migrationBuilder.AlterColumn<decimal>(name: "MaximumAmount", table: "CreditTypes", type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, oldClrType: typeof(decimal), oldType: "numeric(18,2)", oldPrecision: 18, oldScale: 2, oldNullable: true);
        migrationBuilder.AlterColumn<int>(name: "MinimumTermMonths", table: "CreditTypes", type: "integer", nullable: false, oldClrType: typeof(int), oldType: "integer", oldNullable: true);
        migrationBuilder.AlterColumn<int>(name: "MaximumTermMonths", table: "CreditTypes", type: "integer", nullable: false, oldClrType: typeof(int), oldType: "integer", oldNullable: true);
    }
}
