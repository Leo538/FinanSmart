using System.Globalization;
using System.Text;
using Azure;
using Azure.AI.DocumentIntelligence;
using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Entities;
using Microsoft.Extensions.Logging;

namespace FinanSmart.Api.Modules.Investments.Documents;

public class AzureDocumentValidationService(
    DocumentIntelligenceClient client,
    ILogger<AzureDocumentValidationService> logger) : IAzureDocumentValidationService
{
    public async Task<AzureDocumentValidationResult> ValidateAsync(Stream content, InvestmentDocumentType side, InvestmentApplication applicant, CancellationToken cancellationToken = default)
    {
        try
        {
            var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-idDocument", BinaryData.FromStream(content), cancellationToken);
            var document = operation.Value.Documents.FirstOrDefault();
            if (document is null)
                return Result(DocumentValidationStatus.Invalid, "No se identificó un documento de identidad válido.");

            var number = Field(document, "DocumentNumber");
            var firstName = Field(document, "FirstName");
            var lastName = Field(document, "LastName");
            var birthDate = Date(Field(document, "DateOfBirth"));
            var expiration = Date(Field(document, "DateOfExpiration"));
            var country = Field(document, "CountryRegion");
            var type = document.DocumentType;
            var ocrText = operation.Value.Content ?? string.Empty;
            var evaluation = side == InvestmentDocumentType.IdentityFront
                ? EcuadorIdentityDocumentEvaluator.EvaluateFront(ocrText, number, applicant.IdentificationNumber)
                : EcuadorIdentityDocumentEvaluator.EvaluateBack(ocrText);
            logger.LogInformation("Documento analizado: documentos={Documents}, campos={Fields}, cedula={NationalIdFound}, dactilar={FingerprintFound}, estado={Status}", operation.Value.Documents.Count, document.Fields.Count, evaluation.DocumentNumber is not null, evaluation.Status == DocumentValidationStatus.Valid && side == InvestmentDocumentType.IdentityBack, evaluation.Status);
            return Result(evaluation.Status, evaluation.Message, evaluation.DocumentNumber ?? number, firstName, lastName, birthDate, expiration, country, type);
        }
        catch (RequestFailedException exception) when (exception.Status is 408 or 429 or >= 500)
        {
            logger.LogWarning("Document Intelligence no estuvo disponible. Estado: {Status}", exception.Status);
            return Result(DocumentValidationStatus.RequiresManualReview, "No fue posible completar la validación automática. El documento deberá ser revisado.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "No se pudo completar la validación automática del documento.");
            return Result(DocumentValidationStatus.RequiresManualReview, "No fue posible completar la validación automática. El documento deberá ser revisado.");
        }
    }

    private static AzureDocumentValidationResult Result(DocumentValidationStatus status, string message, string? number = null, string? firstName = null, string? lastName = null, DateOnly? birthDate = null, DateOnly? expiration = null, string? country = null, string? type = null) => new()
    {
        Status = status, ValidationMessage = message, ExtractedDocumentNumber = number, ExtractedFirstName = firstName, ExtractedLastName = lastName,
        ExtractedDateOfBirth = birthDate, ExtractedExpirationDate = expiration, ExtractedCountryRegion = country, ExtractedDocumentType = type
    };

    private static string? Field(AnalyzedDocument document, string name) => document.Fields.TryGetValue(name, out var field) ? field.Content?.Trim() : null;
    private static float Confidence(AnalyzedDocument document, string name) => document.Fields.TryGetValue(name, out var field) ? field.Confidence ?? 0 : 0;
    private static DateOnly? Date(string? value) => DateOnly.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
    private static string Normalize(string? value) => string.Concat((value ?? string.Empty).Normalize(NormalizationForm.FormD).Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)).ToUpperInvariant().Replace(" ", string.Empty);
    private static bool SignificantDifference(string? extracted, string? declared) => !string.IsNullOrWhiteSpace(extracted) && !string.IsNullOrWhiteSpace(declared) && Normalize(extracted) != Normalize(declared);
}
