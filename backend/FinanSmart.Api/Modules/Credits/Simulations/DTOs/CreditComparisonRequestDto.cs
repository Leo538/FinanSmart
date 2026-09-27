namespace FinanSmart.Api.Modules.Credits.Simulations.DTOs;

public class CreditComparisonRequestDto
{
    public Guid CreditTypeId { get; init; }
    public decimal Amount { get; init; }
    public int TermMonths { get; init; }
    public DateTimeOffset? StartDate { get; init; }
}

