using FinanSmart.Api.Common.Enums;
namespace FinanSmart.Api.Entities;
public class InvestmentProduct
{
    public Guid Id { get; set; } public string Name { get; set; } = string.Empty; public string? Description { get; set; }
    public decimal MinimumAmount { get; set; } public decimal? MaximumAmount { get; set; }
    public int MinimumTermDays { get; set; } public int? MaximumTermDays { get; set; }
    public InterestCalculationMethod InterestCalculationMethod { get; set; }
    public InterestPaymentFrequency InterestPaymentFrequency { get; set; }
    public bool IsActive { get; set; } public DateTimeOffset CreatedAt { get; set; } public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<InvestmentRate> InvestmentRates { get; set; } = new List<InvestmentRate>();
}
