using FinanSmart.Api.Common.Exceptions;using FinanSmart.Api.Modules.Investments.IdentityVerification.DTOs;using FinanSmart.Api.Modules.Investments.IdentityVerification.Interfaces;using Microsoft.AspNetCore.Mvc;
namespace FinanSmart.Api.Modules.Investments.IdentityVerification.Controllers;
[ApiController][Route("api/investment-applications/{applicationId:guid}/identity-verification")]
public class InvestmentIdentityVerificationController(IInvestmentIdentityVerificationService service):ControllerBase
{
 [HttpGet] public async Task<ActionResult<InvestmentIdentityVerificationDto>> Get(Guid applicationId){var x=await service.GetAsync(applicationId);return x is null?NotFound(new{message="Identity verification was not found."}):Ok(x);}
 [HttpPost("selfie")][Consumes("multipart/form-data")] public async Task<ActionResult<InvestmentIdentityVerificationDto>> Upload(Guid applicationId,[FromForm] IFormFile file){try{return Ok(await service.UploadSelfieAsync(applicationId,file));}catch(IdentityVerificationException e){return StatusCode(e.StatusCode,new{message=e.Message});}}
 [HttpGet("selfie")] public async Task<IActionResult> Selfie(Guid applicationId){var x=await service.GetSelfieAsync(applicationId);return x is null?NotFound(new{message="Selfie was not found."}):File(x.Content,x.ContentType);}
 [HttpPost("verify")] public async Task<ActionResult<InvestmentIdentityVerificationDto>> Verify(Guid applicationId,VerifyIdentityDto dto){try{return Ok(await service.VerifyAsync(applicationId,dto.ConsentAccepted));}catch(IdentityVerificationException e){return StatusCode(e.StatusCode,new{message=e.Message});}}
}
