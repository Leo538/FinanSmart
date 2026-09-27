using FinanSmart.Api.Modules.Credits.Simulations.DTOs;

namespace FinanSmart.Api.Modules.Credits.Simulations.Interfaces;

public interface ICreditSimulationService
{
    Task<CreditSimulationResponseDto> SimulateAsync(CreditSimulationRequestDto request);
}

