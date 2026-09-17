namespace FinanSmart.Api.Modules.Credits.CreditRates.DTOs;

public class CreditRateDto
{
    public Guid Id { get; init; }
    public Guid CreditTypeId { get; init; }
    public string CreditTypeName { get; init; } = string.Empty;
    public decimal AnnualInterestRate { get; init; }
    public DateTimeOffset EffectiveFrom { get; init; }
    public DateTimeOffset? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

