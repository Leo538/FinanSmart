using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Modules.Credits.Simulations.DTOs;
using FinanSmart.Api.Modules.Credits.Simulations.Interfaces;

namespace FinanSmart.Api.Modules.Credits.Simulations.Services;

public class CreditComparisonService(ICreditSimulationService creditSimulationService) : ICreditComparisonService
{
    public async Task<CreditComparisonResponseDto> CompareAsync(CreditComparisonRequestDto request)
    {
        var startDate = request.StartDate ?? DateTimeOffset.UtcNow;
        var french = await creditSimulationService.SimulateAsync(CreateSimulationRequest(request, AmortizationSystem.French, startDate));
        var german = await creditSimulationService.SimulateAsync(CreateSimulationRequest(request, AmortizationSystem.German, startDate));
        var frenchSummary = CreateSummary(french);
        var germanSummary = CreateSummary(german);

        return new CreditComparisonResponseDto
        {
            CreditTypeId = french.CreditTypeId,
            CreditTypeName = french.CreditTypeName,
            RequestedAmount = french.RequestedAmount,
            TermMonths = french.TermMonths,
            AnnualInterestRate = french.AnnualInterestRate,
            StartDate = startDate,
            GeneratedAt = DateTimeOffset.UtcNow,
            French = frenchSummary,
            German = germanSummary,
            InterestDifference = frenchSummary.TotalInterest - germanSummary.TotalInterest,
            ChargesDifference = frenchSummary.TotalCharges - germanSummary.TotalCharges,
            TotalPaymentDifference = frenchSummary.TotalPayment - germanSummary.TotalPayment,
            FirstPaymentDifference = frenchSummary.FirstPayment - germanSummary.FirstPayment,
            LastPaymentDifference = frenchSummary.LastPayment - germanSummary.LastPayment
        };
    }

    private static CreditSimulationRequestDto CreateSimulationRequest(
        CreditComparisonRequestDto request,
        AmortizationSystem amortizationSystem,
        DateTimeOffset startDate) => new()
    {
        CreditTypeId = request.CreditTypeId,
        Amount = request.Amount,
        TermMonths = request.TermMonths,
        AmortizationSystem = amortizationSystem,
        StartDate = startDate
    };

    private static AmortizationComparisonSummaryDto CreateSummary(CreditSimulationResponseDto simulation) => new()
    {
        AmortizationSystem = simulation.AmortizationSystem,
        FirstBasePayment = simulation.BaseFirstPayment,
        LastBasePayment = simulation.BaseLastPayment,
        FirstPayment = simulation.Installments.First().TotalPayment,
        LastPayment = simulation.Installments.Last().TotalPayment,
        TotalPrincipal = simulation.TotalPrincipal,
        TotalInterest = simulation.TotalInterest,
        TotalCharges = simulation.TotalCharges,
        TotalPayment = simulation.TotalPayment
    };
}

