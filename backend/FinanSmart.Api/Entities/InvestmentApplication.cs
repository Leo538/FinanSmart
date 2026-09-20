using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Entities;

public class InvestmentApplication
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public Guid InvestmentProductId { get; set; }
    public string InvestmentProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TermDays { get; set; }
    public decimal AnnualInterestRate { get; set; }
    public InterestCalculationMethod InterestCalculationMethod { get; set; }
    public InterestPaymentFrequency InterestPaymentFrequency { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset MaturityDate { get; set; }
    public decimal TotalInterest { get; set; }
    public decimal PrincipalAtMaturity { get; set; }
    public decimal TotalReceived { get; set; }
    public InvestmentApplicationStatus Status { get; set; }
    public InvestmentApplicationStep CurrentStep { get; set; }
    public string? ApplicantFirstName { get; set; }
    public string? ApplicantLastName { get; set; }
    public IdentificationType? IdentificationType { get; set; }
    public string? IdentificationNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public SourceOfFunds? SourceOfFunds { get; set; }
    public string? OtherSourceOfFunds { get; set; }
    public bool InformationAccuracyAccepted { get; set; }
    public bool TermsAccepted { get; set; }
    public bool DataProcessingAccepted { get; set; }
    public DateTimeOffset? DeclarationsAcceptedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public InvestmentProduct InvestmentProduct { get; set; } = null!;
    public User? User { get; set; }
    public ICollection<InvestmentApplicationDocument> Documents { get; set; } = new List<InvestmentApplicationDocument>();
    public InvestmentIdentityVerification? IdentityVerification { get; set; }
}
