using FinanSmart.Api.Modules.Institution.DTOs;

namespace FinanSmart.Api.Modules.Institution.Interfaces;

public interface IInstitutionService
{
    Task<IReadOnlyCollection<InstitutionDto>> GetAllAsync();
    Task<InstitutionDto?> GetByIdAsync(Guid id);
    Task<InstitutionDto> CreateAsync(CreateInstitutionDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateInstitutionDto dto);
    Task<bool> DeleteAsync(Guid id);
}

