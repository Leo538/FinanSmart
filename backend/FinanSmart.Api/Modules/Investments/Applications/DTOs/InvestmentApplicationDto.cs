using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Investments.Applications.DTOs;

public class InvestmentApplicationDto
{
    public Guid Id { get; init; }
    public string ApplicationNumber { get; init; } = string.Empty;
    public Guid InvestmentProductId { get; init; }
    public string InvestmentProductName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public int TermDays { get; init; }
    public decimal AnnualInterestRate { get; init; }
    public InterestCalculationMethod InterestCalculationMethod { get; init; }
    public InterestPaymentFrequency InterestPaymentFrequency { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset MaturityDate { get; init; }
    public decimal TotalInterest { get; init; }
    public decimal PrincipalAtMaturity { get; init; }
    public decimal TotalReceived { get; init; }
    public InvestmentApplicationStatus Status { get; init; }
    public InvestmentApplicationStep CurrentStep { get; init; }
    public string? ApplicantFirstName { get; init; }
    public string? ApplicantLastName { get; init; }
    public IdentificationType? IdentificationType { get; init; }
    public string? IdentificationNumber { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public DateTimeOffset? SubmittedAt { get; init; }
}
