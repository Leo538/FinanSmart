namespace FinanSmart.Api.Modules.Credits.CreditRates.DTOs;

public class CreateCreditRateDto
{
    public Guid CreditTypeId { get; init; }
    public decimal AnnualInterestRate { get; init; }
    public DateTimeOffset? EffectiveFrom { get; init; }
    public DateTimeOffset? EffectiveTo { get; init; }
    public DateTimeOffset? SourceDate { get; init; }
    public string? SourceUrl { get; init; }
    public bool IsActive { get; init; }
}
