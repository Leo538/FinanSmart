using FinanSmart.Api.Modules.Investments.Products.DTOs;
namespace FinanSmart.Api.Modules.Investments.Products.Interfaces;
public interface IInvestmentProductService { Task<IReadOnlyCollection<InvestmentProductDto>> GetAllAsync(); Task<InvestmentProductDto?> GetByIdAsync(Guid id); Task<InvestmentProductDto> CreateAsync(CreateInvestmentProductDto dto); Task<bool> UpdateAsync(Guid id, UpdateInvestmentProductDto dto); Task<bool> DeleteAsync(Guid id); }
