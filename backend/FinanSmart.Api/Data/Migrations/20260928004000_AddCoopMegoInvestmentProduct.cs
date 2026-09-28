using FinanSmart.Api.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Globalization;

#nullable disable

namespace FinanSmart.Api.Data.Migrations;

[DbContext(typeof(FinanSmartDbContext))]
[Migration("20260928004000_AddCoopMegoInvestmentProduct")]
public partial class AddCoopMegoInvestmentProduct : Migration
{
    private static readonly Guid ProductId = Guid.Parse("d1000000-0000-0000-0000-000000000001");

    protected override void Up(MigrationBuilder migrationBuilder)
    {
        var recordedAt = DateTimeOffset.UtcNow;
        var effectiveFrom = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        migrationBuilder.Sql($"""
            INSERT INTO "InvestmentProducts" ("Id", "Name", "Description", "MinimumAmount", "MaximumAmount", "MinimumTermDays", "MaximumTermDays", "InterestCalculationMethod", "InterestPaymentFrequency", "IsActive", "CreatedAt", "UpdatedAt")
            VALUES ('{ProductId}', 'CoopMego - Depósito a Plazo Pago al Vencimiento', 'Depósito a plazo para personas naturales con pago de intereses al vencimiento. El rango final de plazo publicado como “mayor a 361 días” se representa desde 362 días, por ser un límite exclusivo.', 101.00, NULL, 31, NULL, 0, 0, TRUE, '{recordedAt:O}', '{recordedAt:O}');
            """);

        var amountRanges = new (decimal Minimum, decimal? Maximum)[]
        {
            (101.00m, 5000.99m),
            (5001.00m, 10000.99m),
            (10001.00m, 20000.99m),
            (20001.00m, 50000.99m),
            (50001.00m, 100000.99m),
            (100001.00m, 150000.99m),
            (150001.00m, null)
        };

        // “Mayor a 361” es estricto: el último intervalo inicia en 362, no en 361.
        var termRanges = new (int Minimum, int? Maximum)[]
        {
            (31, 60), (61, 90), (91, 120), (121, 180), (181, 270), (271, 360), (362, null)
        };

        decimal[,] rates =
        {
            { 3.50m, 3.75m, 4.00m, 4.25m, 5.05m, 5.30m, 5.55m },
            { 3.75m, 3.95m, 4.20m, 4.45m, 5.25m, 5.50m, 5.75m },
            { 3.85m, 4.00m, 4.50m, 4.80m, 5.40m, 5.60m, 5.80m },
            { 4.00m, 4.15m, 4.65m, 4.90m, 5.55m, 5.75m, 5.95m },
            { 4.00m, 4.20m, 4.85m, 5.10m, 5.80m, 5.95m, 6.10m },
            { 4.00m, 4.30m, 5.10m, 5.30m, 5.95m, 6.18m, 6.40m },
            { 4.25m, 4.55m, 5.30m, 5.60m, 6.20m, 6.40m, 6.60m }
        };

        var rateNumber = 1;
        for (var amountIndex = 0; amountIndex < amountRanges.Length; amountIndex++)
        {
            for (var termIndex = 0; termIndex < termRanges.Length; termIndex++)
            {
                var rateId = Guid.Parse($"d2000000-0000-0000-0000-{rateNumber++:D12}");
                var maximumAmount = amountRanges[amountIndex].Maximum?.ToString(CultureInfo.InvariantCulture) ?? "NULL";
                var maximumTermDays = termRanges[termIndex].Maximum?.ToString(CultureInfo.InvariantCulture) ?? "NULL";
                var annualRate = rates[amountIndex, termIndex].ToString(CultureInfo.InvariantCulture);

                migrationBuilder.Sql($"""
                    INSERT INTO "InvestmentRates" ("Id", "InvestmentProductId", "MinimumAmount", "MaximumAmount", "MinimumTermDays", "MaximumTermDays", "AnnualInterestRate", "EffectiveFrom", "EffectiveTo", "IsActive", "CreatedAt", "UpdatedAt")
                    VALUES ('{rateId}', '{ProductId}', {amountRanges[amountIndex].Minimum.ToString(CultureInfo.InvariantCulture)}, {maximumAmount}, {termRanges[termIndex].Minimum}, {maximumTermDays}, {annualRate}, '{effectiveFrom:O}', NULL, TRUE, '{recordedAt:O}', '{recordedAt:O}');
                    """);
            }
        }
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql($"""
            DELETE FROM "InvestmentRates" WHERE "InvestmentProductId" = '{ProductId}';
            DELETE FROM "InvestmentProducts" WHERE "Id" = '{ProductId}';
            """);
    }
}
