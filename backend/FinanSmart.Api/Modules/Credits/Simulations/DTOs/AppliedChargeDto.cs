using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Credits.Simulations.DTOs;

public class AppliedChargeDto
{
    public Guid CreditChargeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public ChargeType ChargeType { get; init; }
    public ChargeFrequency Frequency { get; init; }
    public decimal ConfiguredValue { get; init; }
    public decimal CalculatedAmount { get; init; }
}

