using FinanSmart.Api.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanSmart.Api.Data.Migrations;

[DbContext(typeof(FinanSmartDbContext))]
[Migration("20260928001000_ReplaceCreditCatalogueWithVerifiedProducts")]
public partial class ReplaceCreditCatalogueWithVerifiedProducts : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM "CreditTypeInsurances";
            DELETE FROM "CreditCharges";
            DELETE FROM "CreditRates";
            DELETE FROM "CreditTypes";

            INSERT INTO "CreditTypes" ("Id", "Name", "Description", "MinimumAmount", "MaximumAmount", "MinimumTermMonths", "MaximumTermMonths", "IsActive", "CreatedAt", "UpdatedAt") VALUES
            ('a1000000-0000-0000-0000-000000000001', 'Banco del Pacífico - Crédito Ágil', 'Crédito Ágil de Banco del Pacífico: hasta $10.000 y hasta 36 meses. Tasa referencial de 15,60 % reajustable trimestralmente.', NULL, 10000.00, NULL, 36, TRUE, NOW(), NOW()),
            ('a1000000-0000-0000-0000-000000000002', 'Banco del Pacífico - Crédito respaldado por inversión', 'Crédito respaldado por depósito a plazo: hasta el 90 % del valor de la inversión, hasta 5 años y sin seguro de desgravamen. Tasa fija de hasta 11 %, según segmento.', NULL, NULL, NULL, 60, TRUE, NOW(), NOW()),
            ('a1000000-0000-0000-0000-000000000003', 'Banco Guayaquil - Multicrédito', 'Préstamo de libre uso Multicrédito de Banco Guayaquil. La tasa referencial publicada es 15,60 % y la condición definitiva depende de evaluación.', 2000.00, 60000.00, NULL, 60, TRUE, NOW(), NOW()),
            ('a1000000-0000-0000-0000-000000000004', 'Banco Guayaquil - Casafácil Vivienda', 'Crédito Casafácil para vivienda nueva o usada: financiamiento de hasta el 80 % del inmueble y hasta 25 años.', NULL, NULL, NULL, 300, TRUE, NOW(), NOW()),
            ('a1000000-0000-0000-0000-000000000005', 'BanEcuador - PYME Capital de Trabajo', 'Crédito PYME de BanEcuador destinado a capital de trabajo, con periodo de gracia de hasta un año.', 5000.00, 3000000.00, NULL, 36, TRUE, NOW(), NOW()),
            ('a1000000-0000-0000-0000-000000000006', 'BanEcuador - PYME Activo Fijo', 'Crédito PYME de BanEcuador destinado a activo fijo, con periodo de gracia de hasta tres años.', 5000.00, 3000000.00, NULL, 120, TRUE, NOW(), NOW()),
            ('a1000000-0000-0000-0000-000000000007', 'Cooprogreso - Consumo Normal', 'Crédito de Consumo Normal de Cooprogreso.', 600.00, 50000.00, 12, 84, TRUE, NOW(), NOW()),
            ('a1000000-0000-0000-0000-000000000008', 'Cooperativa 29 de Octubre - Consumo Ordinario', 'Crédito de Consumo Ordinario de la Cooperativa 29 de Octubre.', NULL, 100000.00, NULL, 96, TRUE, NOW(), NOW());

            INSERT INTO "CreditRates" ("Id", "CreditTypeId", "AnnualInterestRate", "EffectiveFrom", "EffectiveTo", "IsActive", "CreatedAt", "UpdatedAt") VALUES
            ('b1000000-0000-0000-0000-000000000001', 'a1000000-0000-0000-0000-000000000001', 15.60, '2026-09-28T00:00:00Z', NULL, TRUE, NOW(), NOW()),
            ('b1000000-0000-0000-0000-000000000002', 'a1000000-0000-0000-0000-000000000002', 11.00, '2026-09-28T00:00:00Z', NULL, TRUE, NOW(), NOW()),
            ('b1000000-0000-0000-0000-000000000003', 'a1000000-0000-0000-0000-000000000003', 15.60, '2026-09-28T00:00:00Z', NULL, TRUE, NOW(), NOW()),
            ('b1000000-0000-0000-0000-000000000005', 'a1000000-0000-0000-0000-000000000005', 11.28, '2026-07-03T00:00:00Z', NULL, TRUE, NOW(), NOW()),
            ('b1000000-0000-0000-0000-000000000006', 'a1000000-0000-0000-0000-000000000006', 11.28, '2026-07-03T00:00:00Z', NULL, TRUE, NOW(), NOW()),
            ('b1000000-0000-0000-0000-000000000007', 'a1000000-0000-0000-0000-000000000007', 15.60, '2026-03-01T00:00:00Z', NULL, TRUE, NOW(), NOW()),
            ('b1000000-0000-0000-0000-000000000008', 'a1000000-0000-0000-0000-000000000008', 15.60, '2026-03-03T00:00:00Z', NULL, TRUE, NOW(), NOW());

            INSERT INTO "CreditCharges" ("Id", "CreditTypeId", "Name", "Description", "ChargeType", "Value", "Frequency", "IsActive", "CreatedAt", "UpdatedAt") VALUES
            ('c1000000-0000-0000-0000-000000000001', 'a1000000-0000-0000-0000-000000000001', 'Contribución para atención integral del cáncer', 'Contribución regulada por el SRI para operaciones de crédito del sector financiero privado.', 'PercentageOfPrincipal', 0.50, 'OneTime', TRUE, NOW(), NOW()),
            ('c1000000-0000-0000-0000-000000000002', 'a1000000-0000-0000-0000-000000000002', 'Contribución para atención integral del cáncer', 'Contribución regulada por el SRI.', 'PercentageOfPrincipal', 0.50, 'OneTime', TRUE, NOW(), NOW()),
            ('c1000000-0000-0000-0000-000000000003', 'a1000000-0000-0000-0000-000000000003', 'Contribución para atención integral del cáncer', 'Contribución del 0,5 % regulada por el SRI.', 'PercentageOfPrincipal', 0.50, 'OneTime', TRUE, NOW(), NOW()),
            ('c1000000-0000-0000-0000-000000000004', 'a1000000-0000-0000-0000-000000000004', 'Contribución para atención integral del cáncer', 'Contribución regulada por el SRI.', 'PercentageOfPrincipal', 0.50, 'OneTime', TRUE, NOW(), NOW()),
            ('c1000000-0000-0000-0000-000000000007', 'a1000000-0000-0000-0000-000000000007', 'Contribución para atención integral del cáncer', 'Contribución regulada por el SRI.', 'PercentageOfPrincipal', 0.50, 'OneTime', TRUE, NOW(), NOW()),
            ('c1000000-0000-0000-0000-000000000008', 'a1000000-0000-0000-0000-000000000008', 'Contribución para atención integral del cáncer', 'Contribución regulada por el SRI para operaciones de crédito de cooperativas.', 'PercentageOfPrincipal', 0.50, 'OneTime', TRUE, NOW(), NOW()),
            ('c1000000-0000-0000-0000-000000000009', 'a1000000-0000-0000-0000-000000000007', 'Seguro de desgravamen', '0,90 por mil sobre el saldo inicial de la deuda por cada mes del plazo.', 'PercentageOfPrincipal', 0.09, 'Monthly', TRUE, NOW(), NOW());
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM "CreditTypeInsurances" WHERE "CreditTypeId"::text LIKE 'a1000000-0000-0000-0000-%';
            DELETE FROM "CreditCharges" WHERE "CreditTypeId"::text LIKE 'a1000000-0000-0000-0000-%';
            DELETE FROM "CreditRates" WHERE "CreditTypeId"::text LIKE 'a1000000-0000-0000-0000-%';
            DELETE FROM "CreditTypes" WHERE "Id"::text LIKE 'a1000000-0000-0000-0000-%';
            """);
    }
}
