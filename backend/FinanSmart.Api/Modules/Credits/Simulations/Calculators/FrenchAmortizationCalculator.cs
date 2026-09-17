using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Modules.Credits.Simulations.Interfaces;
using FinanSmart.Api.Modules.Credits.Simulations.Models;

namespace FinanSmart.Api.Modules.Credits.Simulations.Calculators;

public class FrenchAmortizationCalculator : IAmortizationCalculator
{
    public AmortizationSystem System => AmortizationSystem.French;

    public IReadOnlyCollection<AmortizationInstallment> Calculate(
        decimal principal,
        decimal monthlyRate,
        int termMonths,
        DateTimeOffset startDate)
    {
        var installments = new List<AmortizationInstallment>(termMonths);
        var balance = principal;
        var basePayment = monthlyRate == 0
            ? principal / termMonths
            : CalculateBasePayment(principal, monthlyRate, termMonths);

        for (var installmentNumber = 1; installmentNumber <= termMonths; installmentNumber++)
        {
            var openingBalance = balance;
            var interestPayment = openingBalance * monthlyRate;
            var principalPayment = basePayment - interestPayment;
            var payment = basePayment;

            if (installmentNumber == termMonths)
            {
                principalPayment = openingBalance;
                payment = principalPayment + interestPayment;
                balance = 0;
            }
            else
            {
                balance = openingBalance - principalPayment;
            }

            installments.Add(new AmortizationInstallment
            {
                InstallmentNumber = installmentNumber,
                DueDate = startDate.AddMonths(installmentNumber),
                OpeningBalance = openingBalance,
                PrincipalPayment = principalPayment,
                InterestPayment = interestPayment,
                BasePayment = payment,
                ClosingBalance = balance
            });
        }

        return installments;
    }

    private static decimal CalculateBasePayment(decimal principal, decimal monthlyRate, int termMonths)
    {
        var growthFactor = Power(1 + monthlyRate, termMonths);
        return principal * monthlyRate * growthFactor / (growthFactor - 1);
    }

    private static decimal Power(decimal value, int exponent)
    {
        var result = 1m;
        for (var index = 0; index < exponent; index++)
        {
            result *= value;
        }

        return result;
    }
}

