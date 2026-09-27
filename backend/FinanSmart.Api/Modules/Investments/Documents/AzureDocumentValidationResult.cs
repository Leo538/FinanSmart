using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Investments.Documents;

public class AzureDocumentValidationResult
{
    public DocumentValidationStatus Status { get; init; }
    public string? ExtractedDocumentNumber { get; init; }
    public string? ExtractedFirstName { get; init; }
    public string? ExtractedLastName { get; init; }
    public DateOnly? ExtractedDateOfBirth { get; init; }
    public DateOnly? ExtractedExpirationDate { get; init; }
    public string? ExtractedCountryRegion { get; init; }
    public string? ExtractedDocumentType { get; init; }
    public string ValidationMessage { get; init; } = string.Empty;
    public DateTimeOffset ValidatedAt { get; init; } = DateTimeOffset.UtcNow;
}
