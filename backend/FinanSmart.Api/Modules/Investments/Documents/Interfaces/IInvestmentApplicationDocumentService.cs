using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Modules.Investments.Documents.DTOs;
using Microsoft.AspNetCore.Http;

namespace FinanSmart.Api.Modules.Investments.Documents.Interfaces;

public interface IInvestmentApplicationDocumentService
{
    Task<IReadOnlyCollection<InvestmentApplicationDocumentDto>> GetByApplicationAsync(Guid applicationId);
    Task<InvestmentApplicationDocumentDto> UploadAsync(Guid applicationId, InvestmentDocumentType documentType, IFormFile file);
    Task<InvestmentDocumentDownload?> DownloadAsync(Guid applicationId, Guid documentId);
    Task<bool> DeleteAsync(Guid applicationId, Guid documentId);
    Task<InvestmentDocumentRequirementsDto> GetRequirementsAsync(Guid applicationId);
    Task<bool> CompleteDocumentsStepAsync(Guid applicationId);
}
