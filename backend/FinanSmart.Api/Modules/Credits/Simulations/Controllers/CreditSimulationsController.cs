using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Credits.Simulations.DTOs;
using FinanSmart.Api.Modules.Credits.Simulations.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Credits.Simulations.Controllers;

[ApiController]
[Route("api/credit-simulations")]
public class CreditSimulationsController(ICreditSimulationService creditSimulationService) : ControllerBase
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
}

