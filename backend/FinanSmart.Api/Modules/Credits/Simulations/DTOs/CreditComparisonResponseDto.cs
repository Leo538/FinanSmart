namespace FinanSmart.Api.Modules.Credits.Simulations.DTOs;

public class CreditComparisonResponseDto
{
    public Guid CreditTypeId { get; init; }
    public string CreditTypeName { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public int TermMonths { get; init; }
    public decimal AnnualInterestRate { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset GeneratedAt { get; init; }
    public AmortizationComparisonSummaryDto French { get; init; } = null!;
    public AmortizationComparisonSummaryDto German { get; init; } = null!;

    /// <summary>French total interest minus German total interest.</summary>
    public decimal InterestDifference { get; init; }

    /// <summary>French total charges minus German total charges.</summary>
    public decimal ChargesDifference { get; init; }

    /// <summary>French total payment minus German total payment.</summary>
    public decimal TotalPaymentDifference { get; init; }

    /// <summary>French first total payment minus German first total payment.</summary>
    public decimal FirstPaymentDifference { get; init; }

    /// <summary>French last total payment minus German last total payment.</summary>
    public decimal LastPaymentDifference { get; init; }
}

