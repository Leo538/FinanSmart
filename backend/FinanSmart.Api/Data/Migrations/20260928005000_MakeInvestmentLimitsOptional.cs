using FinanSmart.Api.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations;

[DbContext(typeof(FinanSmartDbContext))]
[Migration("20260928005000_MakeInvestmentLimitsOptional")]
public partial class MakeInvestmentLimitsOptional : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<decimal>(
            name: "MinimumAmount", table: "InvestmentProducts", type: "numeric(18,2)", nullable: true,
            oldClrType: typeof(decimal), oldType: "numeric(18,2)");
        migrationBuilder.AlterColumn<int>(
            name: "MinimumTermDays", table: "InvestmentProducts", type: "integer", nullable: true,
            oldClrType: typeof(int), oldType: "integer");
        migrationBuilder.AlterColumn<decimal>(
            name: "MinimumAmount", table: "InvestmentRates", type: "numeric(18,2)", nullable: true,
            oldClrType: typeof(decimal), oldType: "numeric(18,2)");
        migrationBuilder.AlterColumn<int>(
            name: "MinimumTermDays", table: "InvestmentRates", type: "integer", nullable: true,
            oldClrType: typeof(int), oldType: "integer");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => throw new NotSupportedException("No es seguro convertir límites nulos en valores inventados.");
}
