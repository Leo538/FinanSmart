using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using FinanSmart.Api.Modules.Credits.Simulations.Calculators;
using FinanSmart.Api.Modules.Credits.Simulations.DTOs;
using FinanSmart.Api.Modules.Credits.Simulations.Interfaces;
using FinanSmart.Api.Modules.Credits.Simulations.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanSmart.Api.Modules.Credits.Simulations.Services;

public class CreditSimulationService(
    FinanSmartDbContext dbContext,
    IEnumerable<IAmortizationCalculator> amortizationCalculators,
    CreditChargeCalculator creditChargeCalculator) : ICreditSimulationService
{
    public async Task<CreditSimulationResponseDto> SimulateAsync(CreditSimulationRequestDto request)
    {
        ValidateRequest(request);
        var creditType = await dbContext.CreditTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.CreditTypeId);

        if (creditType is null)
        {
            throw new CreditTypeNotFoundException();
        }

        if (!creditType.IsActive)
        {
            throw new ArgumentException("The credit type is inactive.");
        }

        if ((creditType.MinimumAmount.HasValue && request.Amount < creditType.MinimumAmount.Value)
            || (creditType.MaximumAmount.HasValue && request.Amount > creditType.MaximumAmount.Value))
        {
            throw new ArgumentException("Amount is outside the configured range for this credit type.");
        }

        if ((creditType.MinimumTermMonths.HasValue && request.TermMonths < creditType.MinimumTermMonths.Value)
            || (creditType.MaximumTermMonths.HasValue && request.TermMonths > creditType.MaximumTermMonths.Value))
        {
            throw new ArgumentException("Term months are outside the configured range for this credit type.");
        }

        var now = DateTimeOffset.UtcNow;
        var currentRate = await dbContext.CreditRates
            .AsNoTracking()
            .Where(rate => rate.CreditTypeId == creditType.Id
                && rate.IsActive
                && (rate.EffectiveFrom == null || rate.EffectiveFrom <= now)
                && (rate.EffectiveTo == null || rate.EffectiveTo >= now))
            .OrderBy(rate => rate.EffectiveFrom == null)
            .ThenByDescending(rate => rate.EffectiveFrom)
            .FirstOrDefaultAsync();

        if (currentRate is null)
        {
            throw new NoCurrentCreditRateException();
        }

        var calculator = amortizationCalculators.SingleOrDefault(item => item.System == request.AmortizationSystem)
            ?? throw new ArgumentException("Amortization system is invalid.");
        var startDate = request.StartDate ?? now;
        var monthlyRate = currentRate.AnnualInterestRate / 100m / 12m;
        var financialInstallments = calculator.Calculate(request.Amount, monthlyRate, request.TermMonths, startDate);
        var chargesByInstallment = await creditChargeCalculator.CalculateAsync(creditType.Id, request.Amount, financialInstallments);
        var installments = CreateInstallmentDtos(financialInstallments, chargesByInstallment);

        return new CreditSimulationResponseDto
        {
            CreditTypeId = creditType.Id,
            CreditTypeName = creditType.Name,
            RequestedAmount = RoundMoney(request.Amount),
            TermMonths = request.TermMonths,
            AmortizationSystem = request.AmortizationSystem,
            AnnualInterestRate = currentRate.AnnualInterestRate,
            MonthlyInterestRate = monthlyRate,
            StartDate = startDate,
            BaseFirstPayment = RoundMoney(financialInstallments.First().BasePayment),
            BaseLastPayment = RoundMoney(financialInstallments.Last().BasePayment),
            TotalPrincipal = RoundMoney(installments.Sum(item => item.PrincipalPayment)),
            TotalInterest = RoundMoney(installments.Sum(item => item.InterestPayment)),
            TotalCharges = RoundMoney(installments.Sum(item => item.AdditionalCharges)),
            TotalPayment = RoundMoney(installments.Sum(item => item.TotalPayment)),
            Installments = installments,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    private static IReadOnlyCollection<AmortizationInstallmentDto> CreateInstallmentDtos(
        IReadOnlyCollection<AmortizationInstallment> financialInstallments,
        IReadOnlyCollection<IReadOnlyCollection<AppliedChargeDto>> chargesByInstallment)
    {
        var installmentList = financialInstallments.ToList();
        var chargesList = chargesByInstallment.ToList();
        var totalPrincipal = RoundMoney(installmentList.Sum(item => item.PrincipalPayment));
        var totalInterest = RoundMoney(installmentList.Sum(item => item.InterestPayment));
        var results = new List<AmortizationInstallmentDto>(installmentList.Count);

        for (var index = 0; index < installmentList.Count; index++)
        {
            var installment = installmentList[index];
            var charges = chargesList[index];
            var roundedCharges = charges.Select(charge => new AppliedChargeDto
            {
                CreditChargeId = charge.CreditChargeId,
                Name = charge.Name,
                ChargeType = charge.ChargeType,
                Frequency = charge.Frequency,
                ConfiguredValue = charge.ConfiguredValue,
                CalculatedAmount = RoundMoney(charge.CalculatedAmount)
            }).ToList();
            var additionalCharges = roundedCharges.Sum(charge => charge.CalculatedAmount);
            var principalPayment = RoundMoney(installment.PrincipalPayment);
            var interestPayment = RoundMoney(installment.InterestPayment);

            if (index == installmentList.Count - 1)
            {
                principalPayment = totalPrincipal - results.Sum(item => item.PrincipalPayment);
                interestPayment = totalInterest - results.Sum(item => item.InterestPayment);
            }

            var basePayment = RoundMoney(principalPayment + interestPayment);

            results.Add(new AmortizationInstallmentDto
            {
                InstallmentNumber = installment.InstallmentNumber,
                DueDate = installment.DueDate,
                OpeningBalance = RoundMoney(installment.OpeningBalance),
                PrincipalPayment = principalPayment,
                InterestPayment = interestPayment,
                BasePayment = basePayment,
                Charges = roundedCharges,
                AdditionalCharges = additionalCharges,
                TotalPayment = RoundMoney(basePayment + additionalCharges),
                ClosingBalance = RoundMoney(installment.ClosingBalance)
            });
        }

        return results;
    }

    private static void ValidateRequest(CreditSimulationRequestDto request)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }

        if (request.TermMonths <= 0)
        {
            throw new ArgumentException("Term months must be greater than zero.");
        }

        if (!Enum.IsDefined(request.AmortizationSystem))
        {
            throw new ArgumentException("Amortization system is invalid.");
        }
    }

    private static decimal RoundMoney(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
