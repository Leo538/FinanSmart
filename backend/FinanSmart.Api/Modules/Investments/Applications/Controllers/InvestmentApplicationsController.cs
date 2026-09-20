using System.Security.Claims;
using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Investments.Applications.DTOs;
using FinanSmart.Api.Modules.Investments.Applications.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Investments.Applications.Controllers;

[ApiController]
[Authorize]
[Route("api/investment-applications")]
public class InvestmentApplicationsController(IInvestmentApplicationService service) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InvestmentApplicationDto>>> GetAll() => Ok(await service.GetAllAsync());

    [Authorize(Roles = "Client")]
    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyCollection<InvestmentApplicationDto>>> GetMine() => Ok(await service.GetMineAsync(GetUserId()));

    [Authorize(Roles = "Client,Admin")]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvestmentApplicationDto>> GetById(Guid id)
    {
        if (!await HasAccessAsync(id)) return Forbid();
        var application = await service.GetByIdAsync(id);
        return application is null ? NotFound(new { message = "Investment application was not found." }) : Ok(application);
    }

    [Authorize(Roles = "Client")]
    [HttpPost]
    public async Task<ActionResult<InvestmentApplicationDto>> Create(CreateInvestmentApplicationDto dto)
    {
        try
        {
            var application = await service.CreateAsync(dto, GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = application.Id }, application);
        }
        catch (InvestmentProductNotFoundException exception) { return NotFound(new { message = exception.Message }); }
        catch (ApplicableInvestmentRateNotFoundException exception) { return NotFound(new { message = exception.Message }); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpPut("{id:guid}/applicant")]
    public async Task<ActionResult<InvestmentApplicationDto>> UpdateApplicant(Guid id, UpdateInvestmentApplicantDto dto)
    {
        if (!await HasAccessAsync(id)) return Forbid();
        try
        {
            var application = await service.UpdateApplicantAsync(id, dto);
            return application is null ? NotFound(new { message = "Investment application was not found." }) : Ok(application);
        }
        catch (InvestmentApplicationReadOnlyException exception) { return BadRequest(new { message = exception.Message }); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        if (!await HasAccessAsync(id)) return Forbid();
        try { return await service.CancelAsync(id) ? NoContent() : NotFound(new { message = "Investment application was not found." }); }
        catch (InvestmentApplicationReadOnlyException exception) { return BadRequest(new { message = exception.Message }); }
    }

    [Authorize(Roles = "Client,Admin")]
    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<InvestmentApplicationDto>> Submit(Guid id)
    {
        if (!await HasAccessAsync(id)) return Forbid();
        try
        {
            var application = await service.SubmitAsync(id);
            return application is null ? NotFound(new { message = "Investment application was not found." }) : Ok(application);
        }
        catch (InvestmentApplicationReadOnlyException exception) { return BadRequest(new { message = exception.Message }); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/review")]
    public async Task<ActionResult<InvestmentApplicationDto>> Review(Guid id, ReviewInvestmentApplicationDto dto)
    {
        try { var application = await service.ReviewAsync(id, dto); return application is null ? NotFound(new { message = "Investment application was not found." }) : Ok(application); }
        catch (InvestmentApplicationReadOnlyException exception) { return BadRequest(new { message = exception.Message }); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private Task<bool> HasAccessAsync(Guid id) => service.CanAccessAsync(id, GetUserId(), User.IsInRole("Admin"));
}
