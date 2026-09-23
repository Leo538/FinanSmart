using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Investments.Documents;

/// <summary>Aplica reglas de consistencia para cédulas ecuatorianas; no certifica autenticidad legal.</summary>
public static class EcuadorIdentityDocumentEvaluator
{
    private static readonly string[] EcuadorMarkers = ["REPUBLICA DEL ECUADOR", "DIRECCION GENERAL DE REGISTRO CIVIL", "CEDULA", "IDENTIDAD", "CIUDADANIA"];
    private static readonly string[] EcuadorBackMarkers = ["CODIGO DACTILAR", "DIRECTOR GENERAL", "LUGAR Y FECHA DE EMISION", "ECU"];
    private static readonly Regex NationalIdPattern = new(@"(?<!\d)(?:\d[\s-]*){10}(?!\d)", RegexOptions.Compiled);
    private static readonly Regex FingerprintPattern = new(@"(?<![A-Z0-9])[A-Z][A-Z0-9]{4}[A-Z][A-Z0-9]{4}(?![A-Z0-9])", RegexOptions.Compiled);

    public static EcuadorIdentityEvaluation EvaluateFront(string? ocrText, string? mappedNumber, string? applicantNumber)
    {
        var text = ocrText ?? string.Empty;
        if (EvidenceCount(text) < 2) return Invalid();
        var candidates = NationalIdPattern.Matches(text).Select(match => NormalizeDigits(match.Value)).Append(NormalizeDigits(mappedNumber))
            .Where(IsValidNationalId).Distinct(StringComparer.Ordinal).ToArray();
        var declaredNumber = NormalizeDigits(applicantNumber);
        // En cédulas ecuatorianas el campo estructurado puede tomar el "No. documento";
        // si el NUI declarado aparece en el OCR, se prioriza ese candidato de diez dígitos válido.
        if (candidates.Contains(declaredNumber, StringComparer.Ordinal))
            return new(DocumentValidationStatus.Valid, "Cédula frontal validada.", declaredNumber);
        if (candidates.Length != 1) return candidates.Length > 1 ? Review("La cédula contiene números de identificación ambiguos.") : Invalid();
        if (!string.Equals(candidates[0], declaredNumber, StringComparison.Ordinal))
            return new(DocumentValidationStatus.Invalid, "El número de cédula no coincide con el registrado.", candidates[0]);
        return new(DocumentValidationStatus.Valid, "Cédula frontal validada.", candidates[0]);
    }

    public static EcuadorIdentityEvaluation EvaluateBack(string? ocrText)
    {
        var text = ocrText ?? string.Empty;
        if (BackEvidenceCount(text) < 2) return Invalid();
        var hasFingerprint = FingerprintPattern.IsMatch(text.ToUpperInvariant());
        return hasFingerprint
            ? new(DocumentValidationStatus.Valid, "Información posterior del documento verificada.")
            : Review("El documento parece una cédula, pero no se pudo leer el código dactilar del reverso.");
    }

    public static bool IsValidNationalId(string? value)
    {
        var digits = NormalizeDigits(value);
        if (digits.Length != 10 || digits.Any(character => !char.IsAsciiDigit(character))) return false;
        var province = int.Parse(digits[..2], CultureInfo.InvariantCulture);
        if (province is < 1 or > 24 || digits[2] > '5') return false;
        var sum = 0;
        for (var index = 0; index < 9; index++) { var valueAtIndex = digits[index] - '0'; if (index % 2 == 0) valueAtIndex *= 2; sum += valueAtIndex > 9 ? valueAtIndex - 9 : valueAtIndex; }
        return (10 - sum % 10) % 10 == digits[9] - '0';
    }

    private static int EvidenceCount(string text) { var normalized = NormalizeText(text); return EcuadorMarkers.Count(marker => normalized.Contains(marker, StringComparison.Ordinal)); }
    private static int BackEvidenceCount(string text) { var normalized = NormalizeText(text); return EcuadorBackMarkers.Count(marker => normalized.Contains(marker, StringComparison.Ordinal)); }
    private static string NormalizeText(string value) => string.Concat(value.Normalize(NormalizationForm.FormD).Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)).ToUpperInvariant().Replace('\n', ' ').Replace('\r', ' ');
    private static string NormalizeDigits(string? value) => new((value ?? string.Empty).Where(char.IsAsciiDigit).ToArray());
    private static EcuadorIdentityEvaluation Invalid() => new(DocumentValidationStatus.Invalid, "La imagen no corresponde a una cédula de identidad válida para este proceso.");
    private static EcuadorIdentityEvaluation Review(string message) => new(DocumentValidationStatus.RequiresManualReview, message);
}

public sealed record EcuadorIdentityEvaluation(DocumentValidationStatus Status, string Message, string? DocumentNumber = null);
