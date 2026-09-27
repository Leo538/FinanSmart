using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Modules.Credits.Simulations.Models;

namespace FinanSmart.Api.Modules.Credits.Simulations.Interfaces;

public interface IAmortizationCalculator
{
    AmortizationSystem System { get; }
    IReadOnlyCollection<AmortizationInstallment> Calculate(
        decimal principal,
        decimal monthlyRate,
        int termMonths,
        DateTimeOffset startDate);
}

