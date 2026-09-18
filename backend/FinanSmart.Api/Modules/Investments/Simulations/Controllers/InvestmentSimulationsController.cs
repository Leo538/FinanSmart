using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Investments.Simulations.DTOs;
using FinanSmart.Api.Modules.Investments.Simulations.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Investments.Simulations.Controllers;

[ApiController]
[Route("api/investment-simulations")]
public class InvestmentSimulationsController(IInvestmentSimulationService service) : ControllerBase
{
    [HttpPost("simulate")]
    public async Task<ActionResult<InvestmentSimulationResponseDto>> Simulate(InvestmentSimulationRequestDto request)
    {
        try
        {
            return Ok(await service.SimulateAsync(request));
        }
        catch (InvestmentProductNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (ApplicableInvestmentRateNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
