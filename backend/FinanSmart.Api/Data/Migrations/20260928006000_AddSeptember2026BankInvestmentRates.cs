using System.Globalization;
using FinanSmart.Api.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations;

[DbContext(typeof(FinanSmartDbContext))]
[Migration("20260928006000_AddSeptember2026BankInvestmentRates")]
public partial class AddSeptember2026BankInvestmentRates : Migration
{
    private static readonly Guid PichinchaProductId = Guid.Parse("d3000000-0000-0000-0000-000000000001");
    private static readonly Guid PacificoProductId = Guid.Parse("d3000000-0000-0000-0000-000000000002");

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var recordedAt = DateTimeOffset.UtcNow;
        var effectiveFrom = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero);
        var effectiveTo = new DateTimeOffset(2026, 9, 30, 23, 59, 59, TimeSpan.Zero);

        // The digital Pichincha tariff publishes seven amount bands and eight term bands.
        InsertProduct(
            migrationBuilder, PichinchaProductId,
            "Banco Pichincha - Inversión a Plazo Digital",
            "Póliza de acumulación o certificado de depósito a plazo contratado por canales digitales. Tasas nominales del tarifario oficial de septiembre de 2026.",
            500m, null, 31, null, recordedAt);

        (decimal Minimum, decimal? Maximum)[] pichinchaAmounts =
        [
            (500m, 4999.99m), (5000m, 9999.99m), (10000m, 49999.99m),
            (50000m, 99999.99m), (100000m, 199999.99m),
            (200000m, 499999.99m), (500000m, null)
        ];
        (int Minimum, int? Maximum)[] pichinchaTerms =
        [
            (31, 60), (61, 90), (91, 120), (121, 180),
            (181, 240), (241, 300), (301, 360), (361, null)
        ];
        decimal[,] pichinchaRates =
        {
            { 2.65m, 2.85m, 2.95m, 3.25m, 3.50m, 3.85m, 4.05m },
            { 2.85m, 3.05m, 3.20m, 3.50m, 3.75m, 4.00m, 4.20m },
            { 3.05m, 3.25m, 3.45m, 3.75m, 4.00m, 4.20m, 4.35m },
            { 3.35m, 3.55m, 3.80m, 4.05m, 4.20m, 4.40m, 4.55m },
            { 3.65m, 3.85m, 4.00m, 4.25m, 4.40m, 4.60m, 4.65m },
            { 3.90m, 4.10m, 4.25m, 4.45m, 4.55m, 4.80m, 4.85m },
            { 4.10m, 4.25m, 4.45m, 4.65m, 4.75m, 5.05m, 5.10m },
            { 4.20m, 4.35m, 4.55m, 4.75m, 4.90m, 5.30m, 5.35m }
        };

        for (var termIndex = 0; termIndex < pichinchaTerms.Length; termIndex++)
        {
            for (var amountIndex = 0; amountIndex < pichinchaAmounts.Length; amountIndex++)
            {
                var rateNumber = termIndex * pichinchaAmounts.Length + amountIndex + 1;
                InsertRate(migrationBuilder, Guid.Parse($"e3000000-0000-0000-0000-{rateNumber:D12}"),
                    PichinchaProductId, pichinchaAmounts[amountIndex].Minimum,
                    pichinchaAmounts[amountIndex].Maximum, pichinchaTerms[termIndex].Minimum,
                    pichinchaTerms[termIndex].Maximum, pichinchaRates[termIndex, amountIndex],
                    effectiveFrom, effectiveTo, recordedAt);
            }
        }

        // Opening is offered from $200, but the published nominal rate grid begins at $1,000.
        InsertProduct(
            migrationBuilder, PacificoProductId,
            "Banco del Pacífico - Depósito a Plazo",
            "Depósito a plazo fijo. Apertura comercial desde $200; la matriz nominal publicada en septiembre de 2026 empieza en $1.000. Las tasas solo están publicadas para plazos concretos.",
            200m, null, 30, 360, recordedAt);

        (decimal Minimum, decimal? Maximum)[] pacificoAmounts =
        [
            (1000m, 3999.99m), (4000m, 19999.99m), (20000m, 49999.99m),
            (50000m, 99999.99m), (100000m, 499999.99m), (500000m, null)
        ];
        int[] pacificoTerms = [30, 60, 90, 120, 180, 270, 360];
        decimal[,] pacificoRates =
        {
            { 2.10m, 2.25m, 2.50m, 2.75m, 3.20m, 3.50m, 3.85m },
            { 2.25m, 2.45m, 2.90m, 3.05m, 3.55m, 3.85m, 4.10m },
            { 2.35m, 2.55m, 3.00m, 3.50m, 3.75m, 3.95m, 4.20m },
            { 2.45m, 2.65m, 3.25m, 3.60m, 3.85m, 4.05m, 4.30m },
            { 2.55m, 2.75m, 3.45m, 3.75m, 3.95m, 4.25m, 4.40m },
            { 2.75m, 2.95m, 3.55m, 3.85m, 4.05m, 4.35m, 4.50m }
        };

        for (var amountIndex = 0; amountIndex < pacificoAmounts.Length; amountIndex++)
        {
            for (var termIndex = 0; termIndex < pacificoTerms.Length; termIndex++)
            {
                var rateNumber = amountIndex * pacificoTerms.Length + termIndex + 1;
                InsertRate(migrationBuilder, Guid.Parse($"e4000000-0000-0000-0000-{rateNumber:D12}"),
                    PacificoProductId, pacificoAmounts[amountIndex].Minimum,
                    pacificoAmounts[amountIndex].Maximum, pacificoTerms[termIndex],
                    pacificoTerms[termIndex], pacificoRates[amountIndex, termIndex],
                    effectiveFrom, effectiveTo, recordedAt);
            }
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($"DELETE FROM \"InvestmentRates\" WHERE \"InvestmentProductId\" IN ('{PichinchaProductId}', '{PacificoProductId}');");
        migrationBuilder.Sql($"DELETE FROM \"InvestmentProducts\" WHERE \"Id\" IN ('{PichinchaProductId}', '{PacificoProductId}');");
    }

    private static void InsertProduct(MigrationBuilder migrationBuilder, Guid id, string name,
        string description, decimal minimumAmount, decimal? maximumAmount, int minimumTermDays,
        int? maximumTermDays, DateTimeOffset recordedAt)
    {
        var maximumAmountSql = maximumAmount?.ToString(CultureInfo.InvariantCulture) ?? "NULL";
        var maximumTermSql = maximumTermDays?.ToString(CultureInfo.InvariantCulture) ?? "NULL";
        migrationBuilder.Sql($"""
            INSERT INTO "InvestmentProducts" ("Id", "Name", "Description", "MinimumAmount", "MaximumAmount", "MinimumTermDays", "MaximumTermDays", "InterestCalculationMethod", "InterestPaymentFrequency", "IsActive", "CreatedAt", "UpdatedAt")
            VALUES ('{id}', '{name.Replace("'", "''")}', '{description.Replace("'", "''")}', {minimumAmount.ToString(CultureInfo.InvariantCulture)}, {maximumAmountSql}, {minimumTermDays}, {maximumTermSql}, 0, 0, TRUE, '{recordedAt:O}', '{recordedAt:O}');
            """);
    }

    private static void InsertRate(MigrationBuilder migrationBuilder, Guid id, Guid productId,
        decimal minimumAmount, decimal? maximumAmount, int minimumTermDays, int? maximumTermDays,
        decimal annualRate, DateTimeOffset effectiveFrom, DateTimeOffset effectiveTo,
        DateTimeOffset recordedAt)
    {
        var maximumAmountSql = maximumAmount?.ToString(CultureInfo.InvariantCulture) ?? "NULL";
        var maximumTermSql = maximumTermDays?.ToString(CultureInfo.InvariantCulture) ?? "NULL";
        migrationBuilder.Sql($"""
            INSERT INTO "InvestmentRates" ("Id", "InvestmentProductId", "MinimumAmount", "MaximumAmount", "MinimumTermDays", "MaximumTermDays", "AnnualInterestRate", "EffectiveFrom", "EffectiveTo", "IsActive", "CreatedAt", "UpdatedAt")
            VALUES ('{id}', '{productId}', {minimumAmount.ToString(CultureInfo.InvariantCulture)}, {maximumAmountSql}, {minimumTermDays}, {maximumTermSql}, {annualRate.ToString(CultureInfo.InvariantCulture)}, '{effectiveFrom:O}', '{effectiveTo:O}', TRUE, '{recordedAt:O}', '{recordedAt:O}');
            """);
    }
}
