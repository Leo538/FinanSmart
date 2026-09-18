using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Modules.Investments.Rates.Interfaces;
using FinanSmart.Api.Modules.Investments.Simulations.DTOs;
using FinanSmart.Api.Modules.Investments.Simulations.Interfaces;

namespace FinanSmart.Api.Modules.Investments.Simulations.Services;

public class InvestmentSimulationService(FinanSmartDbContext db, IInvestmentRateService rates) : IInvestmentSimulationService
{
    public async Task<InvestmentSimulationResponseDto> SimulateAsync(InvestmentSimulationRequestDto request)
    {
        var product = await db.InvestmentProducts.FindAsync(request.InvestmentProductId)
            ?? throw new InvestmentProductNotFoundException();

        if (!product.IsActive)
            throw new InvestmentProductNotFoundException();

        ValidateRequest(request, product.MinimumAmount, product.MaximumAmount, product.MinimumTermDays, product.MaximumTermDays);

        var rate = await rates.GetApplicableRateAsync(product.Id, request.Amount, request.TermDays)
            ?? throw new ApplicableInvestmentRateNotFoundException();
        var startDate = request.StartDate ?? DateTimeOffset.UtcNow;
        var termFactor = (decimal)request.TermDays / 365m;
        var totalInterest = product.InterestCalculationMethod == InterestCalculationMethod.Simple
            ? request.Amount * (rate.AnnualInterestRate / 100m) * termFactor
            : request.Amount * ((decimal)Math.Pow(1d + (double)(rate.AnnualInterestRate / 100m), (double)termFactor) - 1m);
        var paymentSchedule = BuildPaymentSchedule(startDate, request.TermDays, totalInterest, product.InterestPaymentFrequency);

        return new InvestmentSimulationResponseDto
        {
            InvestmentProductId = product.Id,
            InvestmentProductName = product.Name,
            Principal = request.Amount,
            TermDays = request.TermDays,
            AnnualInterestRate = rate.AnnualInterestRate,
            InterestCalculationMethod = product.InterestCalculationMethod,
            InterestPaymentFrequency = product.InterestPaymentFrequency,
            StartDate = startDate,
            MaturityDate = startDate.AddDays(request.TermDays),
            TotalInterest = totalInterest,
            PrincipalAtMaturity = request.Amount,
            TotalReceived = request.Amount + totalInterest,
            PaymentCount = paymentSchedule.Count,
            PaymentSchedule = paymentSchedule,
            GeneratedAt = DateTimeOffset.UtcNow
        };
    }

    private static void ValidateRequest(InvestmentSimulationRequestDto request, decimal minimumAmount, decimal? maximumAmount, int minimumTermDays, int? maximumTermDays)
    {
        if (request.Amount <= 0 || request.TermDays <= 0 || request.Amount < minimumAmount ||
            (maximumAmount.HasValue && request.Amount > maximumAmount) || request.TermDays < minimumTermDays ||
            (maximumTermDays.HasValue && request.TermDays > maximumTermDays))
            throw new ArgumentException("Invalid investment parameters.");
    }

    private static List<InvestmentPaymentDto> BuildPaymentSchedule(DateTimeOffset startDate, int termDays, decimal totalInterest, InterestPaymentFrequency frequency)
    {
        var maturityDate = startDate.AddDays(termDays);
        if (frequency == InterestPaymentFrequency.Upfront)
            return [new InvestmentPaymentDto { PaymentNumber = 1, PaymentDate = startDate, PaymentType = "Upfront", InterestAmount = totalInterest }];

        var intervalMonths = frequency switch
        {
            InterestPaymentFrequency.Monthly => 1,
            InterestPaymentFrequency.Quarterly => 3,
            InterestPaymentFrequency.SemiAnnual => 6,
            _ => 0
        };
        if (intervalMonths == 0)
            return [new InvestmentPaymentDto { PaymentNumber = 1, PaymentDate = maturityDate, PaymentType = "Maturity", InterestAmount = totalInterest }];

        var paymentDates = new List<DateTimeOffset>();
        for (var date = startDate.AddMonths(intervalMonths); date < maturityDate; date = date.AddMonths(intervalMonths))
            paymentDates.Add(date);
        paymentDates.Add(maturityDate);

        var schedule = new List<InvestmentPaymentDto>();
        var assignedInterest = 0m;
        for (var index = 0; index < paymentDates.Count; index++)
        {
            var interestAmount = index == paymentDates.Count - 1
                ? totalInterest - assignedInterest
                : decimal.Round(totalInterest / paymentDates.Count, 2, MidpointRounding.AwayFromZero);
            assignedInterest += interestAmount;
            schedule.Add(new InvestmentPaymentDto
            {
                PaymentNumber = index + 1,
                PaymentDate = paymentDates[index],
                PaymentType = index == paymentDates.Count - 1 ? "Maturity" : "Interest",
                InterestAmount = interestAmount
            });
        }

        return schedule;
    }
}
