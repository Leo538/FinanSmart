using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Modules.Credits.Simulations.Interfaces;
using FinanSmart.Api.Modules.Credits.Simulations.Models;

namespace FinanSmart.Api.Modules.Credits.Simulations.Calculators;

public class GermanAmortizationCalculator : IAmortizationCalculator
{
    public AmortizationSystem System => AmortizationSystem.German;

    public IReadOnlyCollection<AmortizationInstallment> Calculate(
        decimal principal,
        decimal monthlyRate,
        int termMonths,
        DateTimeOffset startDate)
    {
        var installments = new List<AmortizationInstallment>(termMonths);
        var balance = principal;
        var regularPrincipalPayment = principal / termMonths;

        for (var installmentNumber = 1; installmentNumber <= termMonths; installmentNumber++)
        {
            var openingBalance = balance;
            var interestPayment = openingBalance * monthlyRate;
            var principalPayment = installmentNumber == termMonths ? openingBalance : regularPrincipalPayment;
            var basePayment = principalPayment + interestPayment;
            balance = installmentNumber == termMonths ? 0 : openingBalance - principalPayment;

            installments.Add(new AmortizationInstallment
            {
                InstallmentNumber = installmentNumber,
                DueDate = startDate.AddMonths(installmentNumber),
                OpeningBalance = openingBalance,
                PrincipalPayment = principalPayment,
                InterestPayment = interestPayment,
                BasePayment = basePayment,
                ClosingBalance = balance
            });
        }

        return installments;
    }
}

