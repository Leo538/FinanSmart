using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using FinanSmart.Api.Modules.Credits.CreditRates.DTOs;
using FinanSmart.Api.Modules.Credits.CreditRates.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanSmart.Api.Modules.Credits.CreditRates.Services;

public class CreditRateService(FinanSmartDbContext dbContext) : ICreditRateService
{
    public async Task<IReadOnlyCollection<CreditRateDto>> GetAllAsync()
    {
        var rates = await dbContext.CreditRates
            .AsNoTracking()
            .Include(rate => rate.CreditType)
            .OrderBy(rate => rate.CreditType.Name)
            .ThenBy(rate => rate.EffectiveFrom == null)
            .ThenByDescending(rate => rate.EffectiveFrom)
            .ToListAsync();

        return rates.Select(ToDto).ToList();
    }

    public async Task<CreditRateDto?> GetByIdAsync(Guid id)
    {
        var rate = await dbContext.CreditRates
            .AsNoTracking()
            .Include(rate => rate.CreditType)
            .FirstOrDefaultAsync(rate => rate.Id == id);

        return rate is null ? null : ToDto(rate);
    }

    public async Task<IReadOnlyCollection<CreditRateDto>> GetByCreditTypeAsync(Guid creditTypeId)
    {
        var rates = await dbContext.CreditRates
            .AsNoTracking()
            .Include(rate => rate.CreditType)
            .Where(rate => rate.CreditTypeId == creditTypeId)
            .OrderBy(rate => rate.EffectiveFrom == null)
            .ThenByDescending(rate => rate.EffectiveFrom)
            .ToListAsync();

        return rates.Select(ToDto).ToList();
    }

    public async Task<CreditRateDto?> GetCurrentRateAsync(Guid creditTypeId)
    {
        var now = DateTimeOffset.UtcNow;
        var rate = await dbContext.CreditRates
            .AsNoTracking()
            .Include(rate => rate.CreditType)
            .Where(rate => rate.CreditTypeId == creditTypeId
                && rate.IsActive
                && (rate.EffectiveFrom == null || rate.EffectiveFrom <= now)
                && (rate.EffectiveTo == null || rate.EffectiveTo >= now))
            .OrderBy(rate => rate.EffectiveFrom == null)
            .ThenByDescending(rate => rate.EffectiveFrom)
            .FirstOrDefaultAsync();

        return rate is null ? null : ToDto(rate);
    }

    public async Task<CreditRateDto> CreateAsync(CreateCreditRateDto dto)
    {
        ValidateBusinessRules(dto.AnnualInterestRate, dto.EffectiveFrom, dto.EffectiveTo);
        await EnsureCreditTypeCanReceiveRateAsync(dto.CreditTypeId, true);

        if (dto.IsActive && await HasOverlappingActiveRateAsync(dto.CreditTypeId, dto.EffectiveFrom, dto.EffectiveTo))
        {
            throw new CreditRateOverlapException();
        }

        var now = DateTimeOffset.UtcNow;
        var rate = new CreditRate
        {
            Id = Guid.NewGuid(),
            CreditTypeId = dto.CreditTypeId,
            AnnualInterestRate = dto.AnnualInterestRate,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            SourceDate = dto.SourceDate,
            SourceUrl = dto.SourceUrl?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.CreditRates.Add(rate);
        await dbContext.SaveChangesAsync();

        await dbContext.Entry(rate).Reference(item => item.CreditType).LoadAsync();
        return ToDto(rate);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCreditRateDto dto)
    {
        ValidateBusinessRules(dto.AnnualInterestRate, dto.EffectiveFrom, dto.EffectiveTo);
        var rate = await dbContext.CreditRates.FindAsync(id);

        if (rate is null)
        {
            return false;
        }

        var creditTypeIsChanging = rate.CreditTypeId != dto.CreditTypeId;
        await EnsureCreditTypeCanReceiveRateAsync(dto.CreditTypeId, creditTypeIsChanging);

        if (dto.IsActive && await HasOverlappingActiveRateAsync(dto.CreditTypeId, dto.EffectiveFrom, dto.EffectiveTo, id))
        {
            throw new CreditRateOverlapException();
        }

        rate.CreditTypeId = dto.CreditTypeId;
        rate.AnnualInterestRate = dto.AnnualInterestRate;
        rate.EffectiveFrom = dto.EffectiveFrom;
        rate.EffectiveTo = dto.EffectiveTo;
        rate.SourceDate = dto.SourceDate;
        rate.SourceUrl = dto.SourceUrl?.Trim();
        rate.IsActive = dto.IsActive;
        rate.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var rate = await dbContext.CreditRates.FindAsync(id);

        if (rate is null)
        {
            return false;
        }

        rate.IsActive = false;
        rate.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();
        return true;
    }

    private async Task EnsureCreditTypeCanReceiveRateAsync(Guid creditTypeId, bool requireActive)
    {
        var creditType = await dbContext.CreditTypes.FindAsync(creditTypeId);
        if (creditType is null)
        {
            throw new CreditTypeNotFoundException();
        }

        if (requireActive && !creditType.IsActive)
        {
            throw new ArgumentException("An inactive credit type cannot receive a new rate.");
        }
    }

    private async Task<bool> HasOverlappingActiveRateAsync(
        Guid creditTypeId,
        DateTimeOffset? effectiveFrom,
        DateTimeOffset? effectiveTo,
        Guid? excludedRateId = null)
    {
        if (!effectiveFrom.HasValue)
        {
            return await dbContext.CreditRates.AnyAsync(rate =>
                rate.CreditTypeId == creditTypeId
                && rate.IsActive
                && rate.Id != excludedRateId);
        }

        var proposedEffectiveTo = effectiveTo ?? DateTimeOffset.MaxValue;

        return await dbContext.CreditRates.AnyAsync(rate =>
            rate.CreditTypeId == creditTypeId
            && rate.IsActive
            && rate.Id != excludedRateId
            && (rate.EffectiveFrom == null || rate.EffectiveFrom <= proposedEffectiveTo)
            && (rate.EffectiveTo == null || rate.EffectiveTo >= effectiveFrom.Value));
    }

    private static void ValidateBusinessRules(decimal annualInterestRate, DateTimeOffset? effectiveFrom, DateTimeOffset? effectiveTo)
    {
        if (annualInterestRate <= 0)
        {
            throw new ArgumentException("Annual interest rate must be greater than zero.");
        }

        if (effectiveTo.HasValue && effectiveFrom.HasValue && effectiveTo.Value < effectiveFrom.Value)
        {
            throw new ArgumentException("Effective to must be later than or equal to effective from.");
        }
    }

    private static CreditRateDto ToDto(CreditRate rate) => new()
    {
        Id = rate.Id,
        CreditTypeId = rate.CreditTypeId,
        CreditTypeName = rate.CreditType.Name,
        AnnualInterestRate = rate.AnnualInterestRate,
        EffectiveFrom = rate.EffectiveFrom,
        EffectiveTo = rate.EffectiveTo,
        SourceDate = rate.SourceDate,
        SourceUrl = rate.SourceUrl,
        IsActive = rate.IsActive,
        CreatedAt = rate.CreatedAt,
        UpdatedAt = rate.UpdatedAt
    };
}
