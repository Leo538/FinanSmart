using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Credits.Simulations.DTOs;

public class CreditSimulationResponseDto
{
    public Guid CreditTypeId { get; init; }
    public string CreditTypeName { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public int TermMonths { get; init; }
    public AmortizationSystem AmortizationSystem { get; init; }
    public decimal AnnualInterestRate { get; init; }
    public decimal MonthlyInterestRate { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public decimal BaseFirstPayment { get; init; }
    public decimal BaseLastPayment { get; init; }
    public decimal TotalPrincipal { get; init; }
    public decimal TotalInterest { get; init; }
    public decimal TotalCharges { get; init; }
    public decimal TotalPayment { get; init; }
    public IReadOnlyCollection<AmortizationInstallmentDto> Installments { get; init; } = [];
    public DateTimeOffset GeneratedAt { get; init; }
}

