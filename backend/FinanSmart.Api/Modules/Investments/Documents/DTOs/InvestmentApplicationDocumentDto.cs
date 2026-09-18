using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Investments.Documents.DTOs;

public class InvestmentApplicationDocumentDto
{
    public Guid Id { get; init; }
    public Guid InvestmentApplicationId { get; init; }
    public InvestmentDocumentType DocumentType { get; init; }
    public string OriginalFileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
    public bool IsActive { get; init; }
}
