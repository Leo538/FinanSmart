using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Credits.Simulations.DTOs;

public class CreditSimulationRequestDto
{
    public Guid CreditTypeId { get; init; }
    public decimal Amount { get; init; }
    public int TermMonths { get; init; }
    public AmortizationSystem AmortizationSystem { get; init; }
    public DateTimeOffset? StartDate { get; init; }
}

