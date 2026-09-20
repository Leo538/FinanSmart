using FinanSmart.Api.Modules.Investments.Applications.DTOs;

namespace FinanSmart.Api.Modules.Investments.Applications.Interfaces;

public interface IInvestmentApplicationService
{
    Task<IReadOnlyCollection<InvestmentApplicationDto>> GetAllAsync();
    Task<IReadOnlyCollection<InvestmentApplicationDto>> GetMineAsync(Guid userId);
    Task<InvestmentApplicationDto?> GetByIdAsync(Guid id);
    Task<InvestmentApplicationDto> CreateAsync(CreateInvestmentApplicationDto dto, Guid userId);
    Task<bool> CanAccessAsync(Guid applicationId, Guid userId, bool isAdmin);
    Task<InvestmentApplicationDto?> UpdateApplicantAsync(Guid id, UpdateInvestmentApplicantDto dto);
    Task<bool> CancelAsync(Guid id);
    Task<InvestmentApplicationDto?> SubmitAsync(Guid id);
    Task<InvestmentApplicationDto?> ReviewAsync(Guid id, ReviewInvestmentApplicationDto dto);
}
