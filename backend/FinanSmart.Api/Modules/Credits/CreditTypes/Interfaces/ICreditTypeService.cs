using FinanSmart.Api.Modules.Credits.CreditTypes.DTOs;

namespace FinanSmart.Api.Modules.Credits.CreditTypes.Interfaces;

public interface ICreditTypeService
{
    Task<IReadOnlyCollection<CreditTypeDto>> GetAllAsync();
    Task<CreditTypeDto?> GetByIdAsync(Guid id);
    Task<CreditTypeDto> CreateAsync(CreateCreditTypeDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateCreditTypeDto dto);
    Task<bool> DeleteAsync(Guid id);
}

