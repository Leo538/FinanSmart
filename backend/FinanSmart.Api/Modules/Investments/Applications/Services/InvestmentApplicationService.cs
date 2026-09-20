using System.ComponentModel.DataAnnotations;
using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Common.Validation;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using FinanSmart.Api.Modules.Investments.Applications.DTOs;
using FinanSmart.Api.Modules.Investments.Applications.Interfaces;
using FinanSmart.Api.Modules.Investments.Simulations.DTOs;
using FinanSmart.Api.Modules.Investments.Simulations.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanSmart.Api.Modules.Investments.Applications.Services;

public class InvestmentApplicationService(FinanSmartDbContext db, IInvestmentSimulationService simulationService) : IInvestmentApplicationService
{
    public async Task<IReadOnlyCollection<InvestmentApplicationDto>> GetAllAsync() => await db.InvestmentApplications
        .AsNoTracking().OrderByDescending(application => application.CreatedAt).Select(application => Map(application)).ToListAsync();

    public async Task<IReadOnlyCollection<InvestmentApplicationDto>> GetMineAsync(Guid userId) => await db.InvestmentApplications
        .AsNoTracking().Where(application => application.UserId == userId).OrderByDescending(application => application.CreatedAt)
        .Select(application => Map(application)).ToListAsync();

    public async Task<InvestmentApplicationDto?> GetByIdAsync(Guid id)
    {
        var application = await db.InvestmentApplications.AsNoTracking().FirstOrDefaultAsync(application => application.Id == id);
        return application is null ? null : Map(application);
    }

    public async Task<InvestmentApplicationDto> CreateAsync(CreateInvestmentApplicationDto dto, Guid userId)
    {
        var simulation = await simulationService.SimulateAsync(new InvestmentSimulationRequestDto
        {
            InvestmentProductId = dto.InvestmentProductId,
            Amount = dto.Amount,
            TermDays = dto.TermDays,
            StartDate = dto.StartDate
        });
        var now = DateTimeOffset.UtcNow;
        var application = new InvestmentApplication
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ApplicationNumber = await GenerateApplicationNumberAsync(),
            InvestmentProductId = simulation.InvestmentProductId,
            InvestmentProductName = simulation.InvestmentProductName,
            Amount = simulation.Principal,
            TermDays = simulation.TermDays,
            AnnualInterestRate = simulation.AnnualInterestRate,
            InterestCalculationMethod = simulation.InterestCalculationMethod,
            InterestPaymentFrequency = simulation.InterestPaymentFrequency,
            StartDate = simulation.StartDate,
            MaturityDate = simulation.MaturityDate,
            TotalInterest = simulation.TotalInterest,
            PrincipalAtMaturity = simulation.PrincipalAtMaturity,
            TotalReceived = simulation.TotalReceived,
            Status = InvestmentApplicationStatus.Draft,
            CurrentStep = InvestmentApplicationStep.PersonalInformation,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.InvestmentApplications.Add(application);
        await db.SaveChangesAsync();
        return Map(application);
    }

    public Task<bool> CanAccessAsync(Guid applicationId, Guid userId, bool isAdmin) => db.InvestmentApplications
        .AsNoTracking()
        .AnyAsync(application => application.Id == applicationId && (isAdmin || application.UserId == userId));

    public async Task<InvestmentApplicationDto?> UpdateApplicantAsync(Guid id, UpdateInvestmentApplicantDto dto)
    {
        ValidateApplicant(dto);
        var application = await db.InvestmentApplications.FindAsync(id);
        if (application is null) return null;
        if (application.Status != InvestmentApplicationStatus.Draft || application.CurrentStep != InvestmentApplicationStep.PersonalInformation) throw new InvestmentApplicationReadOnlyException();

        application.ApplicantFirstName = dto.FirstName.Trim();
        application.ApplicantLastName = dto.LastName.Trim();
        application.IdentificationType = dto.IdentificationType;
        application.IdentificationNumber = dto.IdentificationNumber.Trim();
        application.Email = dto.Email.Trim();
        application.Phone = dto.Phone.Trim();
        application.BirthDate = dto.BirthDate;
        application.Address = dto.Address.Trim();
        application.City = dto.City.Trim();
        application.CurrentStep = InvestmentApplicationStep.Documents;
        application.Status = InvestmentApplicationStatus.PendingDocuments;
        application.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return Map(application);
    }

    public async Task<bool> CancelAsync(Guid id)
    {
        var application = await db.InvestmentApplications.FindAsync(id);
        if (application is null) return false;
        if (application.Status == InvestmentApplicationStatus.Cancelled) return true;
        if (application.Status == InvestmentApplicationStatus.Submitted) throw new InvestmentApplicationReadOnlyException();
        application.Status = InvestmentApplicationStatus.Cancelled;
        application.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<InvestmentApplicationDto?> UpdateDeclarationsAsync(Guid id, UpdateInvestmentDeclarationsDto dto)
    {
        ValidateDeclarations(dto);
        var application = await db.InvestmentApplications.FindAsync(id);
        if (application is null) return null;
        if (application.Status != InvestmentApplicationStatus.PendingIdentityVerification || application.CurrentStep != InvestmentApplicationStep.Declarations)
            throw new InvestmentApplicationReadOnlyException();

        var now = DateTimeOffset.UtcNow;
        application.SourceOfFunds = dto.SourceOfFunds;
        application.OtherSourceOfFunds = dto.SourceOfFunds == SourceOfFunds.Other ? dto.OtherSourceOfFunds!.Trim() : null;
        application.InformationAccuracyAccepted = dto.InformationAccuracyAccepted;
        application.TermsAccepted = dto.TermsAccepted;
        application.DataProcessingAccepted = dto.DataProcessingAccepted;
        application.DeclarationsAcceptedAt = now;
        application.CurrentStep = InvestmentApplicationStep.Review;
        application.Status = InvestmentApplicationStatus.ReadyForReview;
        application.UpdatedAt = now;
        await db.SaveChangesAsync();
        return Map(application);
    }

    public async Task<InvestmentApplicationDto?> SubmitAsync(Guid id)
    {
        var application = await db.InvestmentApplications.FindAsync(id);
        if (application is null) return null;
        if (application.Status == InvestmentApplicationStatus.Submitted) return Map(application);
        if (application.Status == InvestmentApplicationStatus.Cancelled) throw new InvestmentApplicationReadOnlyException();
        if (application.Status != InvestmentApplicationStatus.ReadyForReview || application.CurrentStep != InvestmentApplicationStep.Review || !ApplicantIsComplete(application))
            throw new ArgumentException("The investment application does not meet all requirements for submission.");

        var requiredTypes = application.IdentificationType == IdentificationType.Passport
            ? new[] { InvestmentDocumentType.IdentityFront }
            : new[] { InvestmentDocumentType.IdentityFront, InvestmentDocumentType.IdentityBack };
        var uploadedTypes = await db.InvestmentApplicationDocuments.Where(document => document.InvestmentApplicationId == id && document.IsActive)
            .Select(document => document.DocumentType).Distinct().ToListAsync();
        var identityVerified = await db.InvestmentIdentityVerifications.AnyAsync(verification => verification.InvestmentApplicationId == id && verification.Status == IdentityVerificationStatus.Verified && verification.ConsentAccepted);
        if (requiredTypes.Except(uploadedTypes).Any() || !identityVerified || !DeclarationsAreComplete(application))
            throw new ArgumentException("The investment application does not meet all requirements for submission.");

        var now = DateTimeOffset.UtcNow;
        application.Status = InvestmentApplicationStatus.Submitted;
        application.CurrentStep = InvestmentApplicationStep.Confirmation;
        application.SubmittedAt = now;
        application.UpdatedAt = now;
        await db.SaveChangesAsync();
        return Map(application);
    }

    public async Task<InvestmentApplicationDto?> ReviewAsync(Guid id, ReviewInvestmentApplicationDto dto)
    {
        var application = await db.InvestmentApplications.FindAsync(id);
        if (application is null) return null;
        if (application.Status != InvestmentApplicationStatus.Submitted) throw new InvestmentApplicationReadOnlyException();
        if (!Enum.IsDefined(dto.Decision) || (dto.Decision == InvestmentApplicationReviewDecision.Reject && string.IsNullOrWhiteSpace(dto.Notes)))
            throw new ArgumentException(dto.Decision == InvestmentApplicationReviewDecision.Reject ? "A rejection reason is required." : "Invalid review decision.");
        application.Status = dto.Decision == InvestmentApplicationReviewDecision.Approve ? InvestmentApplicationStatus.Approved : InvestmentApplicationStatus.Rejected;
        application.ReviewNotes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        application.ReviewedAt = DateTimeOffset.UtcNow;
        application.UpdatedAt = application.ReviewedAt.Value;
        await db.SaveChangesAsync();
        return Map(application);
    }

    private async Task<string> GenerateApplicationNumberAsync()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var number = $"INV-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
            if (!await db.InvestmentApplications.AnyAsync(application => application.ApplicationNumber == number)) return number;
        }
        throw new InvalidOperationException("Unable to generate an investment application number.");
    }

    private static void ValidateApplicant(UpdateInvestmentApplicantDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName) ||
            string.IsNullOrWhiteSpace(dto.IdentificationNumber) || string.IsNullOrWhiteSpace(dto.Email) ||
            string.IsNullOrWhiteSpace(dto.Phone) || string.IsNullOrWhiteSpace(dto.Address) || string.IsNullOrWhiteSpace(dto.City) ||
            !new EmailAddressAttribute().IsValid(dto.Email) || !Enum.IsDefined(dto.IdentificationType) ||
            (dto.IdentificationType == IdentificationType.NationalId && !EcuadorNationalIdValidator.IsValid(dto.IdentificationNumber)) ||
            (dto.BirthDate is not null && dto.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18)))
            throw new ArgumentException("Invalid applicant information.");
    }

    private static bool ApplicantIsComplete(InvestmentApplication application) =>
        !string.IsNullOrWhiteSpace(application.ApplicantFirstName) && !string.IsNullOrWhiteSpace(application.ApplicantLastName) &&
        application.IdentificationType is not null && !string.IsNullOrWhiteSpace(application.IdentificationNumber) &&
        !string.IsNullOrWhiteSpace(application.Email) && !string.IsNullOrWhiteSpace(application.Phone) &&
        !string.IsNullOrWhiteSpace(application.Address) && !string.IsNullOrWhiteSpace(application.City);

    private static bool DeclarationsAreComplete(InvestmentApplication application) =>
        application.SourceOfFunds is not null && application.InformationAccuracyAccepted && application.TermsAccepted &&
        application.DataProcessingAccepted && application.DeclarationsAcceptedAt is not null &&
        (application.SourceOfFunds != SourceOfFunds.Other || !string.IsNullOrWhiteSpace(application.OtherSourceOfFunds));

    private static void ValidateDeclarations(UpdateInvestmentDeclarationsDto dto)
    {
        if (dto.SourceOfFunds is null || !Enum.IsDefined(dto.SourceOfFunds.Value))
            throw new ArgumentException("A source of funds is required.");
        if (!dto.InformationAccuracyAccepted || !dto.TermsAccepted || !dto.DataProcessingAccepted)
            throw new ArgumentException("All declarations must be accepted.");
        if (dto.SourceOfFunds == SourceOfFunds.Other && string.IsNullOrWhiteSpace(dto.OtherSourceOfFunds))
            throw new ArgumentException("Please specify the other source of funds.");
    }

    private static InvestmentApplicationDto Map(InvestmentApplication application) => new()
    {
        Id = application.Id, ApplicationNumber = application.ApplicationNumber, InvestmentProductId = application.InvestmentProductId,
        InvestmentProductName = application.InvestmentProductName, Amount = application.Amount, TermDays = application.TermDays,
        AnnualInterestRate = application.AnnualInterestRate, InterestCalculationMethod = application.InterestCalculationMethod,
        InterestPaymentFrequency = application.InterestPaymentFrequency, StartDate = application.StartDate, MaturityDate = application.MaturityDate,
        TotalInterest = application.TotalInterest, PrincipalAtMaturity = application.PrincipalAtMaturity, TotalReceived = application.TotalReceived,
        Status = application.Status, CurrentStep = application.CurrentStep, ApplicantFirstName = application.ApplicantFirstName,
        ApplicantLastName = application.ApplicantLastName, IdentificationType = application.IdentificationType,
        IdentificationNumber = application.IdentificationNumber, Email = application.Email, Phone = application.Phone,
        BirthDate = application.BirthDate, Address = application.Address, City = application.City,
        SourceOfFunds = application.SourceOfFunds, OtherSourceOfFunds = application.OtherSourceOfFunds,
        InformationAccuracyAccepted = application.InformationAccuracyAccepted, TermsAccepted = application.TermsAccepted,
        DataProcessingAccepted = application.DataProcessingAccepted, DeclarationsAcceptedAt = application.DeclarationsAcceptedAt,
        CreatedAt = application.CreatedAt, UpdatedAt = application.UpdatedAt, SubmittedAt = application.SubmittedAt, ReviewedAt = application.ReviewedAt, ReviewNotes = application.ReviewNotes
    };
}
