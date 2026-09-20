using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Credits.CreditTypes.DTOs;
using FinanSmart.Api.Modules.Credits.CreditTypes.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FinanSmart.Api.Modules.Credits.CreditTypes.Controllers;

[ApiController]
[Route("api/credit-types")]
public class CreditTypesController(ICreditTypeService creditTypeService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<CreditTypeDto>>> GetAll()
    {
        return Ok(await creditTypeService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CreditTypeDto>> GetById(Guid id)
    {
        var creditType = await creditTypeService.GetByIdAsync(id);
        return creditType is null ? NotFound() : Ok(creditType);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CreditTypeDto>> Create(CreateCreditTypeDto dto)
    {
        try
        {
            var creditType = await creditTypeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = creditType.Id }, creditType);
        }
        catch (CreditTypeNameConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCreditTypeDto dto)
    {
        try
        {
            return await creditTypeService.UpdateAsync(id, dto) ? NoContent() : NotFound();
        }
        catch (CreditTypeNameConflictException exception)
        {
            return Conflict(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await creditTypeService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
