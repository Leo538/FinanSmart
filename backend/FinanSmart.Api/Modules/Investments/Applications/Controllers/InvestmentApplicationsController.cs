using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Investments.Applications.DTOs;
using FinanSmart.Api.Modules.Investments.Applications.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Investments.Applications.Controllers;

[ApiController]
[Route("api/investment-applications")]
public class InvestmentApplicationsController(IInvestmentApplicationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InvestmentApplicationDto>>> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvestmentApplicationDto>> GetById(Guid id)
    {
        var application = await service.GetByIdAsync(id);
        return application is null ? NotFound(new { message = "Investment application was not found." }) : Ok(application);
    }

    [HttpPost]
    public async Task<ActionResult<InvestmentApplicationDto>> Create(CreateInvestmentApplicationDto dto)
    {
        try
        {
            var application = await service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = application.Id }, application);
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

    [HttpPut("{id:guid}/applicant")]
    public async Task<ActionResult<InvestmentApplicationDto>> UpdateApplicant(Guid id, UpdateInvestmentApplicantDto dto)
    {
        try
        {
            var application = await service.UpdateApplicantAsync(id, dto);
            return application is null ? NotFound(new { message = "Investment application was not found." }) : Ok(application);
        }
        catch (InvestmentApplicationReadOnlyException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try { return await service.CancelAsync(id) ? NoContent() : NotFound(new { message = "Investment application was not found." }); }
        catch (InvestmentApplicationReadOnlyException exception) { return BadRequest(new { message = exception.Message }); }
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<ActionResult<InvestmentApplicationDto>> Submit(Guid id)
    {
        try
        {
            var application = await service.SubmitAsync(id);
            return application is null ? NotFound(new { message = "Investment application was not found." }) : Ok(application);
        }
        catch (InvestmentApplicationReadOnlyException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
