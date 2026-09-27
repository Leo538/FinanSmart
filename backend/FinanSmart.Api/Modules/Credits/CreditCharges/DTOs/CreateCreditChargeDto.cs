using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Credits.CreditCharges.DTOs;

public class CreateCreditChargeDto
{
    public Guid CreditTypeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ChargeType ChargeType { get; init; }
    public decimal Value { get; init; }
    public ChargeFrequency Frequency { get; init; }
    public bool IsActive { get; init; }
}

