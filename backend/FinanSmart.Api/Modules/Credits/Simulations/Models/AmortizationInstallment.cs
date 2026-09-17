namespace FinanSmart.Api.Modules.Credits.Simulations.Models;

public class AmortizationInstallment
{
    public int InstallmentNumber { get; init; }
    public DateTimeOffset DueDate { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal PrincipalPayment { get; init; }
    public decimal InterestPayment { get; init; }
    public decimal BasePayment { get; init; }
    public decimal ClosingBalance { get; init; }
}

