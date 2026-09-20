using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Institution.DTOs;
using FinanSmart.Api.Modules.Institution.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FinanSmart.Api.Modules.Institution.Controllers;

[ApiController]
[Route("api/institutions")]
public class InstitutionController(IInstitutionService institutionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InstitutionDto>>> GetAll()
    {
        return Ok(await institutionService.GetAllAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstitutionDto>> GetById(Guid id)
    {
        var institution = await institutionService.GetByIdAsync(id);
        return institution is null ? NotFound() : Ok(institution);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<InstitutionDto>> Create(CreateInstitutionDto dto)
    {
        try
        {
            var institution = await institutionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = institution.Id }, institution);
        }
        catch (InstitutionRucConflictException exception)
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
    public async Task<IActionResult> Update(Guid id, UpdateInstitutionDto dto)
    {
        try
        {
            return await institutionService.UpdateAsync(id, dto) ? NoContent() : NotFound();
        }
        catch (InstitutionRucConflictException exception)
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
        return await institutionService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
