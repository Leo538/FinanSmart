using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Modules.Credits.Simulations.DTOs;
using FinanSmart.Api.Modules.Credits.Simulations.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanSmart.Api.Modules.Credits.Simulations.Calculators;

public class CreditChargeCalculator(FinanSmartDbContext dbContext)
{
    public async Task<IReadOnlyCollection<IReadOnlyCollection<AppliedChargeDto>>> CalculateAsync(
        Guid creditTypeId,
        decimal originalPrincipal,
        IReadOnlyCollection<AmortizationInstallment> installments)
    {
        var configuredCharges = await dbContext.CreditCharges
            .AsNoTracking()
            .Where(charge => charge.CreditTypeId == creditTypeId && charge.IsActive)
            .ToListAsync();

        return installments
            .Select(installment => (IReadOnlyCollection<AppliedChargeDto>)configuredCharges
                .Where(charge => charge.Frequency == ChargeFrequency.Monthly || installment.InstallmentNumber == 1)
                .Select(charge => new AppliedChargeDto
                {
                    CreditChargeId = charge.Id,
                    Name = charge.Name,
                    ChargeType = charge.ChargeType,
                    Frequency = charge.Frequency,
                    ConfiguredValue = charge.Value,
                    // Percentage charges use the financial payment before charges to avoid circular calculations.
                    CalculatedAmount = CalculateAmount(charge.ChargeType, charge.Value, originalPrincipal, installment.BasePayment)
                })
                .ToList())
            .ToList();
    }

    private static decimal CalculateAmount(
        ChargeType chargeType,
        decimal configuredValue,
        decimal originalPrincipal,
        decimal basePayment) => chargeType switch
    {
        ChargeType.FixedAmount => configuredValue,
        ChargeType.PercentageOfPrincipal => originalPrincipal * (configuredValue / 100m),
        ChargeType.PercentageOfInstallment => basePayment * (configuredValue / 100m),
        _ => throw new ArgumentOutOfRangeException(nameof(chargeType))
    };
}

