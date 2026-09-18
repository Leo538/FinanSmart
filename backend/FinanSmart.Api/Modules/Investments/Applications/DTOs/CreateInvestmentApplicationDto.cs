namespace FinanSmart.Api.Modules.Investments.Applications.DTOs;

public class CreateInvestmentApplicationDto
{
    public Guid InvestmentProductId { get; init; }
    public decimal Amount { get; init; }
    public int TermDays { get; init; }
    public DateTimeOffset? StartDate { get; init; }
}
