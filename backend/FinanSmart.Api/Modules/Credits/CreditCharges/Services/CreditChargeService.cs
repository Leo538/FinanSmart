using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using FinanSmart.Api.Modules.Credits.CreditCharges.DTOs;
using FinanSmart.Api.Modules.Credits.CreditCharges.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanSmart.Api.Modules.Credits.CreditCharges.Services;

public class CreditChargeService(FinanSmartDbContext dbContext) : ICreditChargeService
{
    public async Task<IReadOnlyCollection<CreditChargeDto>> GetAllAsync()
    {
        var charges = await dbContext.CreditCharges
            .AsNoTracking()
            .Include(charge => charge.CreditType)
            .OrderBy(charge => charge.CreditType.Name)
            .ThenBy(charge => charge.Name)
            .ToListAsync();

        return charges.Select(ToDto).ToList();
    }

    public async Task<CreditChargeDto?> GetByIdAsync(Guid id)
    {
        var charge = await dbContext.CreditCharges
            .AsNoTracking()
            .Include(charge => charge.CreditType)
            .FirstOrDefaultAsync(charge => charge.Id == id);

        return charge is null ? null : ToDto(charge);
    }

    public async Task<IReadOnlyCollection<CreditChargeDto>> GetByCreditTypeAsync(Guid creditTypeId)
    {
        var charges = await dbContext.CreditCharges
            .AsNoTracking()
            .Include(charge => charge.CreditType)
            .Where(charge => charge.CreditTypeId == creditTypeId)
            .OrderBy(charge => charge.Name)
            .ToListAsync();

        return charges.Select(ToDto).ToList();
    }

    public async Task<CreditChargeDto> CreateAsync(CreateCreditChargeDto dto)
    {
        ValidateBusinessRules(dto.Name, dto.Value, dto.ChargeType, dto.Frequency);
        await EnsureCreditTypeCanReceiveChargeAsync(dto.CreditTypeId, true);
        var normalizedName = dto.Name.Trim();

        if (dto.IsActive && await ActiveNameExistsAsync(dto.CreditTypeId, normalizedName))
        {
            throw new CreditChargeNameConflictException();
        }

        var now = DateTimeOffset.UtcNow;
        var charge = new CreditCharge
        {
            Id = Guid.NewGuid(),
            CreditTypeId = dto.CreditTypeId,
            Name = normalizedName,
            Description = dto.Description,
            ChargeType = dto.ChargeType,
            Value = dto.Value,
            Frequency = dto.Frequency,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.CreditCharges.Add(charge);
        await dbContext.SaveChangesAsync();

        await dbContext.Entry(charge).Reference(item => item.CreditType).LoadAsync();
        return ToDto(charge);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateCreditChargeDto dto)
    {
        ValidateBusinessRules(dto.Name, dto.Value, dto.ChargeType, dto.Frequency);
        var charge = await dbContext.CreditCharges.FindAsync(id);

        if (charge is null)
        {
            return false;
        }

        var creditTypeIsChanging = charge.CreditTypeId != dto.CreditTypeId;
        await EnsureCreditTypeCanReceiveChargeAsync(dto.CreditTypeId, creditTypeIsChanging);
        var normalizedName = dto.Name.Trim();

        if (dto.IsActive && await ActiveNameExistsAsync(dto.CreditTypeId, normalizedName, id))
        {
            throw new CreditChargeNameConflictException();
        }

        charge.CreditTypeId = dto.CreditTypeId;
        charge.Name = normalizedName;
        charge.Description = dto.Description;
        charge.ChargeType = dto.ChargeType;
        charge.Value = dto.Value;
        charge.Frequency = dto.Frequency;
        charge.IsActive = dto.IsActive;
        charge.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var charge = await dbContext.CreditCharges.FindAsync(id);

        if (charge is null)
        {
            return false;
        }

        charge.IsActive = false;
        charge.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();
        return true;
    }

    private async Task EnsureCreditTypeCanReceiveChargeAsync(Guid creditTypeId, bool requireActive)
    {
        var creditType = await dbContext.CreditTypes.FindAsync(creditTypeId);
        if (creditType is null)
        {
            throw new CreditTypeNotFoundException();
        }

        if (requireActive && !creditType.IsActive)
        {
            throw new ArgumentException("An inactive credit type cannot receive a new charge.");
        }
    }

    private async Task<bool> ActiveNameExistsAsync(Guid creditTypeId, string name, Guid? excludedChargeId = null)
    {
        var normalizedName = name.ToUpper();
        return await dbContext.CreditCharges.AnyAsync(charge =>
            charge.CreditTypeId == creditTypeId
            && charge.IsActive
            && charge.Id != excludedChargeId
            && charge.Name.ToUpper() == normalizedName);
    }

    private static void ValidateBusinessRules(string name, decimal value, ChargeType chargeType, ChargeFrequency frequency)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.");
        }

        if (value <= 0)
        {
            throw new ArgumentException("Value must be greater than zero.");
        }

        if (!Enum.IsDefined(chargeType))
        {
            throw new ArgumentException("Charge type is invalid.");
        }

        if (!Enum.IsDefined(frequency))
        {
            throw new ArgumentException("Charge frequency is invalid.");
        }
    }

    private static CreditChargeDto ToDto(CreditCharge charge) => new()
    {
        Id = charge.Id,
        CreditTypeId = charge.CreditTypeId,
        CreditTypeName = charge.CreditType.Name,
        Name = charge.Name,
        Description = charge.Description,
        ChargeType = charge.ChargeType,
        Value = charge.Value,
        Frequency = charge.Frequency,
        IsActive = charge.IsActive,
        CreatedAt = charge.CreatedAt,
        UpdatedAt = charge.UpdatedAt
    };
}

