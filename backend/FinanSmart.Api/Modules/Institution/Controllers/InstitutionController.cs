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
    [HttpPost("{id:guid}/logo")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<InstitutionDto>> UploadLogo(Guid id, [FromForm] IFormFile file, [FromServices] FinanSmart.Api.Data.Context.FinanSmartDbContext dbContext, [FromServices] IWebHostEnvironment environment)
    {
        if (file.Length == 0 || file.Length > 2 * 1024 * 1024)
            return BadRequest(new { message = "El logo debe pesar entre 1 byte y 2 MB." });

        var extensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["image/jpeg"] = ".jpg", ["image/png"] = ".png", ["image/webp"] = ".webp"
        };
        if (!extensions.TryGetValue(file.ContentType, out var extension))
            return BadRequest(new { message = "Solo se permiten imágenes JPG, PNG o WEBP." });

        var institution = await dbContext.Institutions.FindAsync(id);
        if (institution is null) return NotFound();

        var relativeDirectory = Path.Combine("Storage", "institution-logos", id.ToString());
        var directory = Path.Combine(environment.ContentRootPath, relativeDirectory);
        Directory.CreateDirectory(directory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        await using (var output = System.IO.File.Create(Path.Combine(directory, fileName))) await file.CopyToAsync(output);

        institution.LogoUrl = "/storage/institution-logos/" + id + "/" + fileName;
        institution.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();
        return Ok(await institutionService.GetByIdAsync(id));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}/logo")]
    public async Task<IActionResult> DeleteLogo(Guid id, [FromServices] FinanSmart.Api.Data.Context.FinanSmartDbContext dbContext)
    {
        var institution = await dbContext.Institutions.FindAsync(id);
        if (institution is null) return NotFound();
        institution.LogoUrl = null;
        institution.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        return await institutionService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
