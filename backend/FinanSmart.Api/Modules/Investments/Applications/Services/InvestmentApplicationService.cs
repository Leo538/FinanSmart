using System.ComponentModel.DataAnnotations;
using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Common.Exceptions;
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

    public async Task<InvestmentApplicationDto?> GetByIdAsync(Guid id)
    {
        var application = await db.InvestmentApplications.AsNoTracking().FirstOrDefaultAsync(application => application.Id == id);
        return application is null ? null : Map(application);
    }

    public async Task<InvestmentApplicationDto> CreateAsync(CreateInvestmentApplicationDto dto)
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

    public async Task<InvestmentApplicationDto?> UpdateApplicantAsync(Guid id, UpdateInvestmentApplicantDto dto)
    {
        ValidateApplicant(dto);
        var application = await db.InvestmentApplications.FindAsync(id);
        if (application is null) return null;
        if (application.Status == InvestmentApplicationStatus.Cancelled) throw new InvestmentApplicationReadOnlyException();

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
        application.Status = InvestmentApplicationStatus.Cancelled;
        application.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return true;
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
            !new EmailAddressAttribute().IsValid(dto.Email) || !Enum.IsDefined(dto.IdentificationType))
            throw new ArgumentException("Invalid applicant information.");
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
        CreatedAt = application.CreatedAt, UpdatedAt = application.UpdatedAt, SubmittedAt = application.SubmittedAt
    };
}
