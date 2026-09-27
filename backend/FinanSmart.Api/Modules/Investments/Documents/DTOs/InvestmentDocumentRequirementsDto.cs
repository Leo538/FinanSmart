using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Investments.Documents.DTOs;

public class InvestmentDocumentRequirementsDto
{
    public IReadOnlyCollection<InvestmentDocumentType> RequiredTypes { get; init; } = [];
    public IReadOnlyCollection<InvestmentDocumentType> UploadedTypes { get; init; } = [];
    public IReadOnlyCollection<InvestmentDocumentType> MissingTypes { get; init; } = [];
    public bool IsComplete { get; init; }
}
