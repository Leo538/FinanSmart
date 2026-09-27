using FinanSmart.Api.Modules.Credits.CreditRates.DTOs;

namespace FinanSmart.Api.Modules.Credits.CreditRates.Interfaces;

public interface ICreditRateService
{
    Task<IReadOnlyCollection<CreditRateDto>> GetAllAsync();
    Task<CreditRateDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<CreditRateDto>> GetByCreditTypeAsync(Guid creditTypeId);
    Task<CreditRateDto?> GetCurrentRateAsync(Guid creditTypeId);
    Task<CreditRateDto> CreateAsync(CreateCreditRateDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateCreditRateDto dto);
    Task<bool> DeleteAsync(Guid id);
}

