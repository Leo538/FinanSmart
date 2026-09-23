using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Entities;

namespace FinanSmart.Api.Modules.Investments.Documents;

public interface IAzureDocumentValidationService
{
    Task<AzureDocumentValidationResult> ValidateAsync(Stream content, InvestmentDocumentType side, InvestmentApplication applicant, CancellationToken cancellationToken = default);
}
