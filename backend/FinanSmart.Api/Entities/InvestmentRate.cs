namespace FinanSmart.Api.Entities;
public class InvestmentRate
{
    public Guid Id { get; set; } public Guid InvestmentProductId { get; set; } public InvestmentProduct InvestmentProduct { get; set; } = null!;
    public decimal? MinimumAmount { get; set; } public decimal? MaximumAmount { get; set; }
    public int? MinimumTermDays { get; set; } public int? MaximumTermDays { get; set; }
    public decimal AnnualInterestRate { get; set; } public DateTimeOffset EffectiveFrom { get; set; } public DateTimeOffset? EffectiveTo { get; set; }
    public string? SourceName { get; set; } public string? SourceUrl { get; set; } public DateOnly? SourceDate { get; set; }
    public bool IsActive { get; set; } public DateTimeOffset CreatedAt { get; set; } public DateTimeOffset UpdatedAt { get; set; }
}
