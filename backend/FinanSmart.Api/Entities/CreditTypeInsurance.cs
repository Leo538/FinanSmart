namespace FinanSmart.Api.Entities;

public class CreditTypeInsurance
{
    public Guid CreditTypeId { get; set; }
    public Guid CreditInsuranceId { get; set; }
    public bool IsRequired { get; set; }
    public CreditType CreditType { get; set; } = null!;
    public CreditInsurance CreditInsurance { get; set; } = null!;
}
