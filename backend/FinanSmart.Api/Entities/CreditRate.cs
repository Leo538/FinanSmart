namespace FinanSmart.Api.Entities;

public class CreditRate
{
    public Guid Id { get; set; }
    public Guid CreditTypeId { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
    public DateTimeOffset? SourceDate { get; set; }
    public string? SourceUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public CreditType CreditType { get; set; } = null!;
}
