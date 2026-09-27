using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using FinanSmart.Api.Modules.Credits.CreditTypes.DTOs;
using FinanSmart.Api.Modules.Credits.CreditTypes.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanSmart.Api.Modules.Credits.CreditTypes.Services;

public class CreditTypeService(FinanSmartDbContext dbContext) : ICreditTypeService
{
    public async Task<IReadOnlyCollection<CreditTypeDto>> GetAllAsync()
    {
        var creditTypes = await dbContext.CreditTypes
            .AsNoTracking()
            .OrderBy(creditType => creditType.Name)
            .ToListAsync();

        return creditTypes.Select(ToDto).ToList();
    }

    public async Task<CreditTypeDto?> GetByIdAsync(Guid id)
    {
        var creditType = await dbContext.CreditTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(creditType => creditType.Id == id);

        return creditType is null ? null : ToDto(creditType);
    }

    public async Task<CreditTypeDto> CreateAsync(CreateCreditTypeDto dto)
    {
        ValidateBusinessRules(dto.Name, dto.MinimumAmount, dto.MaximumAmount, dto.MinimumTermMonths, dto.MaximumTermMonths);
        var normalizedName = dto.Name.Trim();

        if (await NameExistsAsync(normalizedName))
        {
            throw new CreditTypeNameConflictException();
        }

        var now = DateTimeOffset.UtcNow;
        var creditType = new CreditType
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Description = dto.Description,
            MinimumAmount = dto.MinimumAmount,
            MaximumAmount = dto.MaximumAmount,
            MinimumTermMonths = dto.MinimumTermMonths,
            MaximumTermMonths = dto.MaximumTermMonths,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.CreditTypes.Add(creditType);
        await dbContext.SaveChangesAsync();

        return ToDto(creditType);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCreditTypeDto dto)
    {
        ValidateBusinessRules(dto.Name, dto.MinimumAmount, dto.MaximumAmount, dto.MinimumTermMonths, dto.MaximumTermMonths);
        var creditType = await dbContext.CreditTypes.FindAsync(id);

        if (creditType is null)
        {
            return false;
        }

        var normalizedName = dto.Name.Trim();
        if (await NameExistsAsync(normalizedName, id))
        {
            throw new CreditTypeNameConflictException();
        }

        creditType.Name = normalizedName;
        creditType.Description = dto.Description;
        creditType.MinimumAmount = dto.MinimumAmount;
        creditType.MaximumAmount = dto.MaximumAmount;
        creditType.MinimumTermMonths = dto.MinimumTermMonths;
        creditType.MaximumTermMonths = dto.MaximumTermMonths;
        creditType.IsActive = dto.IsActive;
        creditType.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var creditType = await dbContext.CreditTypes.FindAsync(id);

        if (creditType is null)
        {
            return false;
        }

        creditType.IsActive = false;
        creditType.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();

        return true;
    }

    private async Task<bool> NameExistsAsync(string name, Guid? excludedId = null)
    {
        var normalizedName = name.ToUpper();
        return await dbContext.CreditTypes.AnyAsync(creditType =>
            creditType.Id != excludedId && creditType.Name.ToUpper() == normalizedName);
    }

    private static void ValidateBusinessRules(
        string name,
        decimal minimumAmount,
        decimal maximumAmount,
        int minimumTermMonths,
        int maximumTermMonths)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.");
        }

        if (minimumAmount <= 0)
        {
            throw new ArgumentException("Minimum amount must be greater than zero.");
        }

        if (maximumAmount < minimumAmount)
        {
            throw new ArgumentException("Maximum amount must be greater than or equal to minimum amount.");
        }

        if (minimumTermMonths <= 0)
        {
            throw new ArgumentException("Minimum term months must be greater than zero.");
        }

        if (maximumTermMonths < minimumTermMonths)
        {
            throw new ArgumentException("Maximum term months must be greater than or equal to minimum term months.");
        }
    }

    private static CreditTypeDto ToDto(CreditType creditType) => new()
    {
        Id = creditType.Id,
        Name = creditType.Name,
        Description = creditType.Description,
        MinimumAmount = creditType.MinimumAmount,
        MaximumAmount = creditType.MaximumAmount,
        MinimumTermMonths = creditType.MinimumTermMonths,
        MaximumTermMonths = creditType.MaximumTermMonths,
        IsActive = creditType.IsActive,
        CreatedAt = creditType.CreatedAt,
        UpdatedAt = creditType.UpdatedAt
    };
}

