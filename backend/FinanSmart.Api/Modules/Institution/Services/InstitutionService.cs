using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using FinanSmart.Api.Modules.Institution.DTOs;
using FinanSmart.Api.Modules.Institution.Interfaces;
using Microsoft.EntityFrameworkCore;
using InstitutionEntity = FinanSmart.Api.Entities.Institution;

namespace FinanSmart.Api.Modules.Institution.Services;

public class InstitutionService(FinanSmartDbContext dbContext) : IInstitutionService
{
    public async Task<IReadOnlyCollection<InstitutionDto>> GetAllAsync()
    {
        var institutions = await dbContext.Institutions
            .AsNoTracking()
            .OrderBy(institution => institution.Name)
            .ToListAsync();

        return institutions.Select(ToDto).ToList();
    }

    public async Task<InstitutionDto?> GetByIdAsync(Guid id)
    {
        var institution = await dbContext.Institutions
            .AsNoTracking()
            .Where(institution => institution.Id == id)
            .FirstOrDefaultAsync();

        return institution is null ? null : ToDto(institution);
    }

    public async Task<InstitutionDto> CreateAsync(CreateInstitutionDto dto)
    {
        ValidateRequiredFields(dto.Name, dto.Ruc); ValidateBackgroundColor(dto.BackgroundColor);
        var normalizedRuc = dto.Ruc.Trim();

        if (await dbContext.Institutions.AnyAsync(institution => institution.Ruc == normalizedRuc))
        {
            throw new InstitutionRucConflictException();
        }

        var now = DateTimeOffset.UtcNow;
        var institution = new InstitutionEntity
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Ruc = normalizedRuc,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            LogoUrl = dto.LogoUrl,
            PrimaryColor = dto.PrimaryColor,
            SecondaryColor = dto.SecondaryColor,
            BackgroundColor = dto.BackgroundColor,
            IsActive = dto.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Institutions.Add(institution);
        await dbContext.SaveChangesAsync();

        return ToDto(institution);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateInstitutionDto dto)
    {
        ValidateRequiredFields(dto.Name, dto.Ruc); ValidateBackgroundColor(dto.BackgroundColor);
        var institution = await dbContext.Institutions.FindAsync(id);

        if (institution is null)
        {
            return false;
        }

        var normalizedRuc = dto.Ruc.Trim();
        var rucExists = await dbContext.Institutions
            .AnyAsync(existingInstitution => existingInstitution.Id != id && existingInstitution.Ruc == normalizedRuc);

        if (rucExists)
        {
            throw new InstitutionRucConflictException();
        }

        institution.Name = dto.Name.Trim();
        institution.Ruc = normalizedRuc;
        institution.Email = dto.Email;
        institution.Phone = dto.Phone;
        institution.Address = dto.Address;
        institution.LogoUrl = dto.LogoUrl;
        institution.PrimaryColor = dto.PrimaryColor;
        institution.SecondaryColor = dto.SecondaryColor;
        institution.BackgroundColor = dto.BackgroundColor;
        institution.IsActive = dto.IsActive;
        institution.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var institution = await dbContext.Institutions.FindAsync(id);

        if (institution is null)
        {
            return false;
        }

        institution.IsActive = false;
        institution.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();

        return true;
    }

    private static void ValidateRequiredFields(string name, string ruc)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(ruc))
        {
            throw new ArgumentException("RUC is required.");
        }
    }

    private static InstitutionDto ToDto(InstitutionEntity institution) => new()
    {
        Id = institution.Id,
        Name = institution.Name,
        Ruc = institution.Ruc,
        Email = institution.Email,
        Phone = institution.Phone,
        Address = institution.Address,
        LogoUrl = institution.LogoUrl,
        PrimaryColor = institution.PrimaryColor,
        SecondaryColor = institution.SecondaryColor,
        BackgroundColor = institution.BackgroundColor,
        IsActive = institution.IsActive,
        CreatedAt = institution.CreatedAt,
        UpdatedAt = institution.UpdatedAt
    };

    private static void ValidateBackgroundColor(string? color)
    {
        if (string.IsNullOrWhiteSpace(color)) return;
        if (!System.Text.RegularExpressions.Regex.IsMatch(color, "^#[0-9A-Fa-f]{6}$")) throw new ArgumentException("Background color must be a hexadecimal value.");
    }
}
