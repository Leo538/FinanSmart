using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Credits.CreditCharges.DTOs;

public class CreditChargeDto
{
    public Guid Id { get; init; }
    public Guid CreditTypeId { get; init; }
    public string CreditTypeName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ChargeType ChargeType { get; init; }
    public decimal Value { get; init; }
    public ChargeFrequency Frequency { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

