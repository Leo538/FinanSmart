using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Credits.CreditCharges.DTOs;
using FinanSmart.Api.Modules.Credits.CreditCharges.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Credits.CreditCharges.Controllers;

[ApiController]
[Route("api/credit-charges")]
public class CreditChargesController(ICreditChargeService creditChargeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CreditChargeDto>>> GetAll()
    {
        return Ok(await creditChargeService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CreditChargeDto>> GetById(Guid id)
    {
        var charge = await creditChargeService.GetByIdAsync(id);
        return charge is null ? NotFound() : Ok(charge);
    }

    [HttpGet("credit-type/{creditTypeId:guid}")]
    public async Task<ActionResult<IReadOnlyCollection<CreditChargeDto>>> GetByCreditType(Guid creditTypeId)
    {
        return Ok(await creditChargeService.GetByCreditTypeAsync(creditTypeId));
    }

    [HttpPost]
    public async Task<ActionResult<CreditChargeDto>> Create(CreateCreditChargeDto dto)
    {
        try
        {
            var charge = await creditChargeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = charge.Id }, charge);
        }
        catch (CreditTypeNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (CreditChargeNameConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCreditChargeDto dto)
    {
        try
        {
            return await creditChargeService.UpdateAsync(id, dto) ? NoContent() : NotFound();
        }
        catch (CreditTypeNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (CreditChargeNameConflictException exception)
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
        return await creditChargeService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}

