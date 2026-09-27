using System.Security.Claims;
using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Investments.Applications.Interfaces;
using FinanSmart.Api.Modules.Investments.IdentityVerification.DTOs;
using FinanSmart.Api.Modules.Investments.IdentityVerification.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Investments.IdentityVerification.Controllers;

[ApiController]
[Authorize(Roles = "Client,Admin")]
[Route("api/investment-applications/{applicationId:guid}/identity-verification")]
public class InvestmentIdentityVerificationController(IInvestmentIdentityVerificationService service, IInvestmentApplicationService applicationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<InvestmentIdentityVerificationDto>> Get(Guid applicationId)
    {
        if (!await HasAccessAsync(applicationId)) return Forbid();
        var result = await service.GetAsync(applicationId);
        return result is null ? NotFound(new { message = "Identity verification was not found." }) : Ok(result);
    }

    [HttpPost("selfie")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<InvestmentIdentityVerificationDto>> Upload(Guid applicationId, [FromForm] IFormFile file)
    {
        if (!await HasAccessAsync(applicationId)) return Forbid();
        try { return Ok(await service.UploadSelfieAsync(applicationId, file)); }
        catch (IdentityVerificationException exception) { return StatusCode(exception.StatusCode, new { message = exception.Message }); }
    }

    [HttpGet("selfie")]
    public async Task<IActionResult> Selfie(Guid applicationId)
    {
        if (!await HasAccessAsync(applicationId)) return Forbid();
        var selfie = await service.GetSelfieAsync(applicationId);
        return selfie is null ? NotFound(new { message = "Selfie was not found." }) : File(selfie.Content, selfie.ContentType);
    }

    [HttpPost("verify")]
    public async Task<ActionResult<InvestmentIdentityVerificationDto>> Verify(Guid applicationId, VerifyIdentityDto dto)
    {
        if (!await HasAccessAsync(applicationId)) return Forbid();
        try { return Ok(await service.VerifyAsync(applicationId, dto.ConsentAccepted)); }
        catch (IdentityVerificationException exception) { return StatusCode(exception.StatusCode, new { message = exception.Message }); }
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private Task<bool> HasAccessAsync(Guid id) => applicationService.CanAccessAsync(id, GetUserId(), User.IsInRole("Admin"));
}
