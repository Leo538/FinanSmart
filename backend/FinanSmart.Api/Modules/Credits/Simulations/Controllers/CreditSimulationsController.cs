using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Credits.Simulations.DTOs;
using FinanSmart.Api.Modules.Credits.Simulations.Interfaces;
using FinanSmart.Api.Modules.Credits.Simulations.Pdf;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Credits.Simulations.Controllers;

[ApiController]
[Route("api/credit-simulations")]
public class CreditSimulationsController(
    ICreditSimulationService creditSimulationService,
    ICreditSimulationPdfService creditSimulationPdfService) : ControllerBase
{
    [HttpPost("simulate")]
    public async Task<ActionResult<CreditSimulationResponseDto>> Simulate(CreditSimulationRequestDto request)
    {
        try
        {
            return Ok(await creditSimulationService.SimulateAsync(request));
        }
        catch (CreditTypeNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (NoCurrentCreditRateException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("pdf")]
    public async Task<IActionResult> GeneratePdf(CreditSimulationRequestDto request)
    {
        try
        {
            var pdf = await creditSimulationPdfService.GenerateAsync(request);
            var fileName = $"FinanSmart_Simulacion_Credito_{DateTimeOffset.UtcNow:yyyy-MM-dd}.pdf";
            return File(pdf, "application/pdf", fileName);
        }
        catch (CreditTypeNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (NoCurrentCreditRateException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
