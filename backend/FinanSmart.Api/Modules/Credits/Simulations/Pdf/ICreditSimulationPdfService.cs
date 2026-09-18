using FinanSmart.Api.Modules.Credits.Simulations.DTOs;

namespace FinanSmart.Api.Modules.Credits.Simulations.Pdf;

public interface ICreditSimulationPdfService
{
    Task<byte[]> GenerateAsync(CreditSimulationRequestDto request);
}
