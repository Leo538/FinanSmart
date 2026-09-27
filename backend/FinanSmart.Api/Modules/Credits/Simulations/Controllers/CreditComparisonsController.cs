using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Credits.Simulations.DTOs;
using FinanSmart.Api.Modules.Credits.Simulations.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Credits.Simulations.Controllers;

[ApiController]
[Route("api/credit-comparisons")]
public class CreditComparisonsController(ICreditComparisonService creditComparisonService) : ControllerBase
{
    [HttpPost("compare")]
    public async Task<ActionResult<CreditComparisonResponseDto>> Compare(CreditComparisonRequestDto request)
    {
        try
        {
            return Ok(await creditComparisonService.CompareAsync(request));
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

