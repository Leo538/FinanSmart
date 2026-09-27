using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Entities;

public class InvestmentApplicationDocument
{
    public Guid Id { get; set; }
    public Guid InvestmentApplicationId { get; set; }
    public InvestmentDocumentType DocumentType { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public DocumentValidationStatus? ValidationStatus { get; set; }
    public string? ExtractedDocumentNumber { get; set; }
    public string? ExtractedFirstName { get; set; }
    public string? ExtractedLastName { get; set; }
    public DateOnly? ExtractedDateOfBirth { get; set; }
    public DateOnly? ExtractedExpirationDate { get; set; }
    public string? ExtractedCountryRegion { get; set; }
    public string? ExtractedDocumentType { get; set; }
    public string? ValidationMessage { get; set; }
    public DateTimeOffset? ValidatedAt { get; set; }
    public InvestmentApplication InvestmentApplication { get; set; } = null!;
}
