using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using FinanSmart.Api.Modules.Investments.Documents.DTOs;
using FinanSmart.Api.Modules.Investments.Documents.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace FinanSmart.Api.Modules.Investments.Documents.Services;

public class InvestmentApplicationDocumentService(FinanSmartDbContext db, IWebHostEnvironment environment, IAzureDocumentValidationService azureValidationService) : IInvestmentApplicationDocumentService
{
    private const long MaximumFileSize = 5 * 1024 * 1024;
    private static readonly IReadOnlyDictionary<string, string> AllowedFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png", [".pdf"] = "application/pdf"
    };

    public async Task<IReadOnlyCollection<InvestmentApplicationDocumentDto>> GetByApplicationAsync(Guid applicationId)
    {
        await EnsureApplicationExists(applicationId);
        return await db.InvestmentApplicationDocuments.AsNoTracking().Where(document => document.InvestmentApplicationId == applicationId && document.IsActive)
            .OrderBy(document => document.DocumentType).ThenByDescending(document => document.UploadedAt).Select(document => Map(document)).ToListAsync();
    }

    public async Task<InvestmentApplicationDocumentDto> UploadAsync(Guid applicationId, InvestmentDocumentType documentType, IFormFile file)
    {
        var application = await EnsureApplicationExists(applicationId);
        EnsureEditable(application);
        ValidateFile(documentType, file);
        await ValidateImageContentAsync(file);
        if (documentType is InvestmentDocumentType.IdentityFront or InvestmentDocumentType.IdentityBack)
            await EnsureDifferentFromOppositeSideAsync(applicationId, documentType, file);
        AzureDocumentValidationResult? validation = null;
        if (documentType is InvestmentDocumentType.IdentityFront or InvestmentDocumentType.IdentityBack)
        {
            await using var validationStream = file.OpenReadStream();
            validation = await azureValidationService.ValidateAsync(validationStream, documentType, application);
            if (validation.Status != DocumentValidationStatus.Valid)
                throw new InvestmentDocumentValidationException(validation.ValidationMessage, 422);
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine(applicationId.ToString(), storedFileName);
        var directory = Path.Combine(environment.ContentRootPath, "Storage", "InvestmentDocuments", applicationId.ToString());
        Directory.CreateDirectory(directory);
        var fullPath = Path.Combine(directory, storedFileName);
        await using (var stream = File.Create(fullPath)) await file.CopyToAsync(stream);

        if (documentType is InvestmentDocumentType.IdentityFront or InvestmentDocumentType.IdentityBack)
        {
            var previousDocuments = await db.InvestmentApplicationDocuments.Where(document => document.InvestmentApplicationId == applicationId && document.DocumentType == documentType && document.IsActive).ToListAsync();
            foreach (var previous in previousDocuments) { previous.IsActive = false; previous.DeletedAt = DateTimeOffset.UtcNow; }
        }
        var document = new InvestmentApplicationDocument
        {
            Id = Guid.NewGuid(), InvestmentApplicationId = applicationId, DocumentType = documentType,
            OriginalFileName = Path.GetFileName(file.FileName), StoredFileName = storedFileName, ContentType = AllowedFiles[extension],
            FileSize = file.Length, StoragePath = relativePath, IsActive = true, UploadedAt = DateTimeOffset.UtcNow
        };
        if (validation is not null) ApplyValidation(document, validation);
        db.InvestmentApplicationDocuments.Add(document);
        await db.SaveChangesAsync();
        return Map(document);
    }

    public async Task<InvestmentDocumentDownload?> DownloadAsync(Guid applicationId, Guid documentId)
    {
        var document = await db.InvestmentApplicationDocuments.AsNoTracking().FirstOrDefaultAsync(document => document.Id == documentId && document.InvestmentApplicationId == applicationId && document.IsActive);
        if (document is null) return null;
        var root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "Storage", "InvestmentDocuments"));
        var path = Path.GetFullPath(Path.Combine(root, document.StoragePath));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return null;
        return new InvestmentDocumentDownload(File.OpenRead(path), document.ContentType, document.OriginalFileName);
    }

    public async Task<bool> DeleteAsync(Guid applicationId, Guid documentId)
    {
        var application = await EnsureApplicationExists(applicationId);
        EnsureEditable(application);
        var document = await db.InvestmentApplicationDocuments.FirstOrDefaultAsync(document => document.Id == documentId && document.InvestmentApplicationId == applicationId && document.IsActive);
        if (document is null) return false;
        document.IsActive = false;
        document.DeletedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<InvestmentDocumentRequirementsDto> GetRequirementsAsync(Guid applicationId)
    {
        var application = await EnsureApplicationExists(applicationId);
        return await BuildRequirementsAsync(application);
    }

    public async Task<bool> CompleteDocumentsStepAsync(Guid applicationId)
    {
        var application = await db.InvestmentApplications.FindAsync(applicationId);
        if (application is null) return false;
        EnsureEditable(application);
        var requirements = await BuildRequirementsAsync(application);
        if (!requirements.IsComplete) throw new InvestmentDocumentValidationException("Required documents are missing or have not been validated.");
        application.CurrentStep = InvestmentApplicationStep.IdentityVerification;
        application.Status = InvestmentApplicationStatus.PendingIdentityVerification;
        application.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return true;
    }

    private async Task<InvestmentApplication> EnsureApplicationExists(Guid applicationId) => await db.InvestmentApplications.FindAsync(applicationId) ?? throw new InvestmentDocumentValidationException("Investment application was not found.", 404);
    private static void EnsureEditable(InvestmentApplication application)
    {
        if (application.Status == InvestmentApplicationStatus.Cancelled) throw new InvestmentDocumentValidationException("A cancelled investment application is read-only.");
        if (application.CurrentStep != InvestmentApplicationStep.Documents) throw new InvestmentDocumentValidationException("Documents cannot be modified in the current application step.");
    }
    private static void ValidateFile(InvestmentDocumentType documentType, IFormFile file)
    {
        if (!Enum.IsDefined(documentType) || file is null || file.Length == 0) throw new InvestmentDocumentValidationException("The document does not meet the requirements.");
        if (file.Length > MaximumFileSize) throw new InvestmentDocumentValidationException("The file exceeds the maximum allowed size.", 413);
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedFiles.TryGetValue(extension, out var expectedContentType) || !string.Equals(file.ContentType, expectedContentType, StringComparison.OrdinalIgnoreCase))
            throw new InvestmentDocumentValidationException("The file type is not allowed.", 415);
        if (documentType is InvestmentDocumentType.IdentityFront or InvestmentDocumentType.IdentityBack && extension is ".pdf")
            throw new InvestmentDocumentValidationException("Para la cédula frontal y reverso carga una fotografía JPG o PNG, no un PDF.", 415);
    }
    private static async Task ValidateImageContentAsync(IFormFile file)
    {
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) return;
        await using var stream = file.OpenReadStream();
        var header = new byte[12];
        var read = await stream.ReadAsync(header);
        var isJpeg = read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        var isPng = read >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 &&
                    header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A;
        if (!isJpeg && !isPng) throw new InvestmentDocumentValidationException("The image file is invalid.", 415);
    }
    private async Task EnsureDifferentFromOppositeSideAsync(Guid applicationId, InvestmentDocumentType documentType, IFormFile file)
    {
        var oppositeType = documentType == InvestmentDocumentType.IdentityFront ? InvestmentDocumentType.IdentityBack : InvestmentDocumentType.IdentityFront;
        var opposite = await db.InvestmentApplicationDocuments.AsNoTracking().FirstOrDefaultAsync(document => document.InvestmentApplicationId == applicationId && document.DocumentType == oppositeType && document.IsActive);
        if (opposite is null) return;
        var root = Path.GetFullPath(Path.Combine(environment.ContentRootPath, "Storage", "InvestmentDocuments"));
        var path = Path.GetFullPath(Path.Combine(root, opposite.StoragePath));
        if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return;
        await using var incoming = file.OpenReadStream();
        await using var existing = File.OpenRead(path);
        if (await SameSha256Async(incoming, existing)) throw new InvestmentDocumentValidationException("La fotografía frontal y reversa no pueden ser el mismo archivo.", 422);
    }
    private static async Task<bool> SameSha256Async(Stream first, Stream second)
    {
        var firstHash = await SHA256.HashDataAsync(first);
        var secondHash = await SHA256.HashDataAsync(second);
        return CryptographicOperations.FixedTimeEquals(firstHash, secondHash);
    }
    private async Task<InvestmentDocumentRequirementsDto> BuildRequirementsAsync(InvestmentApplication application)
    {
        if (application.IdentificationType is null) throw new InvestmentDocumentValidationException("Applicant information is required before documents.");
        var required = application.IdentificationType == IdentificationType.Passport
            ? new[] { InvestmentDocumentType.IdentityFront }
            : new[] { InvestmentDocumentType.IdentityFront, InvestmentDocumentType.IdentityBack };
        var uploaded = await db.InvestmentApplicationDocuments.AsNoTracking().Where(document => document.InvestmentApplicationId == application.Id && document.IsActive && (document.DocumentType == InvestmentDocumentType.AdditionalDocument || document.ValidationStatus == DocumentValidationStatus.Valid))
            .Select(document => document.DocumentType).Distinct().ToListAsync();
        var missing = required.Except(uploaded).ToArray();
        return new InvestmentDocumentRequirementsDto { RequiredTypes = required, UploadedTypes = uploaded, MissingTypes = missing, IsComplete = missing.Length == 0 };
    }
    private static void ApplyValidation(InvestmentApplicationDocument document, AzureDocumentValidationResult result)
    {
        document.ValidationStatus = result.Status; document.ExtractedDocumentNumber = result.ExtractedDocumentNumber; document.ExtractedFirstName = result.ExtractedFirstName; document.ExtractedLastName = result.ExtractedLastName; document.ExtractedDateOfBirth = result.ExtractedDateOfBirth; document.ExtractedExpirationDate = result.ExtractedExpirationDate; document.ExtractedCountryRegion = result.ExtractedCountryRegion; document.ExtractedDocumentType = result.ExtractedDocumentType; document.ValidationMessage = result.ValidationMessage; document.ValidatedAt = result.ValidatedAt;
    }
    private static InvestmentApplicationDocumentDto Map(InvestmentApplicationDocument document) => new() { Id = document.Id, InvestmentApplicationId = document.InvestmentApplicationId, DocumentType = document.DocumentType, OriginalFileName = document.OriginalFileName, ContentType = document.ContentType, FileSize = document.FileSize, UploadedAt = document.UploadedAt, IsActive = document.IsActive, ValidationStatus = document.ValidationStatus, ExtractedDocumentNumber = document.ExtractedDocumentNumber, ExtractedFirstName = document.ExtractedFirstName, ExtractedLastName = document.ExtractedLastName, ExtractedDateOfBirth = document.ExtractedDateOfBirth, ValidationMessage = document.ValidationMessage, ValidatedAt = document.ValidatedAt };
}
