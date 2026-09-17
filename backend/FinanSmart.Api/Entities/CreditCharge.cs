using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Entities;

public class CreditCharge
{
    public Guid Id { get; set; }
    public Guid CreditTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ChargeType ChargeType { get; set; }
    public decimal Value { get; set; }
    public ChargeFrequency Frequency { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public CreditType CreditType { get; set; } = null!;
}

