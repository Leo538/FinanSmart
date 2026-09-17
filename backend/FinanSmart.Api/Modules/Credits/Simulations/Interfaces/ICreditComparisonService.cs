using FinanSmart.Api.Modules.Credits.Simulations.DTOs;

namespace FinanSmart.Api.Modules.Credits.Simulations.Interfaces;

public interface ICreditComparisonService
{
    Task<CreditComparisonResponseDto> CompareAsync(CreditComparisonRequestDto request);
}

