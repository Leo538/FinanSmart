namespace FinanSmart.Api.Modules.Credits.CreditRates.DTOs;

public class UpdateCreditRateDto
{
    public Guid CreditTypeId { get; init; }
    public decimal AnnualInterestRate { get; init; }
    public DateTimeOffset EffectiveFrom { get; init; }
    public DateTimeOffset? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
}

