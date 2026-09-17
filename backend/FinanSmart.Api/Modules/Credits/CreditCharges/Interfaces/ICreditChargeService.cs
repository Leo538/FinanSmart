using FinanSmart.Api.Modules.Credits.CreditCharges.DTOs;

namespace FinanSmart.Api.Modules.Credits.CreditCharges.Interfaces;

public interface ICreditChargeService
{
    Task<IReadOnlyCollection<CreditChargeDto>> GetAllAsync();
    Task<CreditChargeDto?> GetByIdAsync(Guid id);
    Task<IReadOnlyCollection<CreditChargeDto>> GetByCreditTypeAsync(Guid creditTypeId);
    Task<CreditChargeDto> CreateAsync(CreateCreditChargeDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateCreditChargeDto dto);
    Task<bool> DeleteAsync(Guid id);
}

