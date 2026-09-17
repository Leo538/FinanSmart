using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Credits.Simulations.DTOs;

public class AmortizationComparisonSummaryDto
{
    public AmortizationSystem AmortizationSystem { get; init; }
    public decimal FirstBasePayment { get; init; }
    public decimal LastBasePayment { get; init; }

    /// <summary>Total payment for the first installment, including additional charges.</summary>
    public decimal FirstPayment { get; init; }

    /// <summary>Total payment for the last installment, including additional charges.</summary>
    public decimal LastPayment { get; init; }

    public decimal TotalPrincipal { get; init; }
    public decimal TotalInterest { get; init; }
    public decimal TotalCharges { get; init; }
    public decimal TotalPayment { get; init; }
}

