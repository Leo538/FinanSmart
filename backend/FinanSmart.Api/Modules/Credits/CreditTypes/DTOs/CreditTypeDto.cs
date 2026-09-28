namespace FinanSmart.Api.Modules.Credits.CreditTypes.DTOs;

public class CreditTypeDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal? MinimumAmount { get; init; }
    public decimal? MaximumAmount { get; init; }
    public int? MinimumTermMonths { get; init; }
    public int? MaximumTermMonths { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
