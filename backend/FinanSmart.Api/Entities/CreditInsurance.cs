using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Entities;

public class CreditInsurance
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public InsuranceType InsuranceType { get; set; }
    public InsuranceCalculationBasis CalculationBasis { get; set; }
    public decimal? Rate { get; set; }
    public decimal? FixedAmount { get; set; }
    public InsuranceFrequency Frequency { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<CreditTypeInsurance> CreditTypeInsurances { get; set; } = new List<CreditTypeInsurance>();
}
