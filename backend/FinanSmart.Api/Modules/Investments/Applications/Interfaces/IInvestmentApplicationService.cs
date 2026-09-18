using FinanSmart.Api.Modules.Investments.Applications.DTOs;

namespace FinanSmart.Api.Modules.Investments.Applications.Interfaces;

public interface IInvestmentApplicationService
{
    Task<IReadOnlyCollection<InvestmentApplicationDto>> GetAllAsync();
    Task<InvestmentApplicationDto?> GetByIdAsync(Guid id);
    Task<InvestmentApplicationDto> CreateAsync(CreateInvestmentApplicationDto dto);
    Task<InvestmentApplicationDto?> UpdateApplicantAsync(Guid id, UpdateInvestmentApplicantDto dto);
    Task<bool> CancelAsync(Guid id);
}
