namespace FinanSmart.Api.Entities;

public class CreditType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MinimumAmount { get; set; }
    public decimal MaximumAmount { get; set; }
    public int MinimumTermMonths { get; set; }
    public int MaximumTermMonths { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<CreditRate> CreditRates { get; set; } = new List<CreditRate>();
    public ICollection<CreditCharge> CreditCharges { get; set; } = new List<CreditCharge>();
}

