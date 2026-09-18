using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Modules.Investments.Documents.DTOs;
using FinanSmart.Api.Modules.Investments.Documents.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinanSmart.Api.Modules.Investments.Documents.Controllers;

[ApiController]
[Route("api/investment-applications/{applicationId:guid}/documents")]
public class InvestmentApplicationDocumentsController(IInvestmentApplicationDocumentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InvestmentApplicationDocumentDto>>> GetAll(Guid applicationId)
    {
        try { return Ok(await service.GetByApplicationAsync(applicationId)); }
        catch (InvestmentDocumentValidationException exception) { return StatusCode(exception.StatusCode, new { message = exception.Message }); }
    }

    [HttpGet("requirements")]
    public async Task<ActionResult<InvestmentDocumentRequirementsDto>> GetRequirements(Guid applicationId)
    {
        try { return Ok(await service.GetRequirementsAsync(applicationId)); }
        catch (InvestmentDocumentValidationException exception) { return StatusCode(exception.StatusCode, new { message = exception.Message }); }
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<InvestmentApplicationDocumentDto>> Upload(Guid applicationId, [FromForm] InvestmentDocumentType documentType, [FromForm] IFormFile file)
    {
        try { return Ok(await service.UploadAsync(applicationId, documentType, file)); }
        catch (InvestmentDocumentValidationException exception) { return StatusCode(exception.StatusCode, new { message = exception.Message }); }
    }

    [HttpGet("{documentId:guid}/download")]
    public async Task<IActionResult> Download(Guid applicationId, Guid documentId)
    {
        var download = await service.DownloadAsync(applicationId, documentId);
        if (download is null) return NotFound(new { message = "Document or application was not found." });
        return File(download.Content, download.ContentType, download.OriginalFileName);
    }

    [HttpDelete("{documentId:guid}")]
    public async Task<IActionResult> Delete(Guid applicationId, Guid documentId)
    {
        try { return await service.DeleteAsync(applicationId, documentId) ? NoContent() : NotFound(new { message = "Document was not found." }); }
        catch (InvestmentDocumentValidationException exception) { return StatusCode(exception.StatusCode, new { message = exception.Message }); }
    }

    [HttpPost("complete")]
    public async Task<IActionResult> Complete(Guid applicationId)
    {
        try { return await service.CompleteDocumentsStepAsync(applicationId) ? NoContent() : NotFound(new { message = "Investment application was not found." }); }
        catch (InvestmentDocumentValidationException exception) { return StatusCode(exception.StatusCode, new { message = exception.Message }); }
    }
}
