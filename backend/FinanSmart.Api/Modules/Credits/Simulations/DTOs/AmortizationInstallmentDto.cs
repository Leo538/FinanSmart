namespace FinanSmart.Api.Modules.Credits.Simulations.DTOs;

public class AmortizationInstallmentDto
{
    public int InstallmentNumber { get; init; }
    public DateTimeOffset DueDate { get; init; }
    public decimal OpeningBalance { get; init; }
    public decimal PrincipalPayment { get; init; }
    public decimal InterestPayment { get; init; }
    public decimal BasePayment { get; init; }
    public IReadOnlyCollection<AppliedChargeDto> Charges { get; init; } = [];
    public decimal AdditionalCharges { get; init; }
    public decimal TotalPayment { get; init; }
    public decimal ClosingBalance { get; init; }
}

