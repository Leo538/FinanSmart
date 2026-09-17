using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Credits.CreditRates.DTOs;
using FinanSmart.Api.Modules.Credits.CreditRates.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Credits.CreditRates.Controllers;

[ApiController]
[Route("api/credit-rates")]
public class CreditRatesController(ICreditRateService creditRateService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CreditRateDto>>> GetAll()
    {
        return Ok(await creditRateService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CreditRateDto>> GetById(Guid id)
    {
        var rate = await creditRateService.GetByIdAsync(id);
        return rate is null ? NotFound() : Ok(rate);
    }

    [HttpGet("credit-type/{creditTypeId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<CreditRateDto>>> GetByCreditType(Guid creditTypeId)
    {
        return Ok(await creditRateService.GetByCreditTypeAsync(creditTypeId));
    }

    [HttpGet("credit-type/{creditTypeId:guid}/current")]
    public async Task<ActionResult<CreditRateDto>> GetCurrentRate(Guid creditTypeId)
    {
        var rate = await creditRateService.GetCurrentRateAsync(creditTypeId);
        return rate is null ? NotFound() : Ok(rate);
    }

    [HttpPost]
    public async Task<ActionResult<CreditRateDto>> Create(CreateCreditRateDto dto)
    {
        try
        {
            var rate = await creditRateService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = rate.Id }, rate);
        }
        catch (CreditTypeNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (CreditRateOverlapException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCreditRateDto dto)
    {
        try
        {
            return await creditRateService.UpdateAsync(id, dto) ? NoContent() : NotFound();
        }
        catch (CreditTypeNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (CreditRateOverlapException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await creditRateService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

