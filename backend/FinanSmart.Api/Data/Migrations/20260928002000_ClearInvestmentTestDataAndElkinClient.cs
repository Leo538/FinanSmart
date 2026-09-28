using FinanSmart.Api.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations;

[DbContext(typeof(FinanSmartDbContext))]
[Migration("20260928002000_ClearInvestmentTestDataAndElkinClient")]
public partial class ClearInvestmentTestDataAndElkinClient : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM "InvestmentIdentityVerifications";
            DELETE FROM "InvestmentApplicationDocuments";
            DELETE FROM "InvestmentApplications";
            DELETE FROM "InvestmentRates";
            DELETE FROM "InvestmentProducts";

            DELETE FROM "RefreshTokens"
            WHERE "UserId" IN (
                SELECT "Id" FROM "Users"
                WHERE "FirstName" = 'Elkin' AND "LastName" = 'López'
            );

            DELETE FROM "UserRoles"
            WHERE "UserId" IN (
                SELECT "Id" FROM "Users"
                WHERE "FirstName" = 'Elkin' AND "LastName" = 'López'
            );

            DELETE FROM "Users"
            WHERE "FirstName" = 'Elkin' AND "LastName" = 'López';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Los datos de prueba eliminados no se pueden reconstruir de forma fiable.
    }
}
