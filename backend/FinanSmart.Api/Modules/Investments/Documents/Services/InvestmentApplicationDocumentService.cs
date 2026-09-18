using FinanSmart.Api.Common.Enums;
using FinanSmart.Api.Common.Exceptions;
using FinanSmart.Api.Data.Context;
using FinanSmart.Api.Entities;
using FinanSmart.Api.Modules.Investments.Documents.DTOs;
using FinanSmart.Api.Modules.Investments.Documents.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FinanSmart.Api.Modules.Investments.Documents.Services;

public class InvestmentApplicationDocumentService(FinanSmartDbContext db, IWebHostEnvironment environment) : IInvestmentApplicationDocumentService
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
        if (!requirements.IsComplete) throw new InvestmentDocumentValidationException("Required documents are missing.");
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
    }
    private async Task<InvestmentDocumentRequirementsDto> BuildRequirementsAsync(InvestmentApplication application)
    {
        if (application.IdentificationType is null) throw new InvestmentDocumentValidationException("Applicant information is required before documents.");
        var required = application.IdentificationType == IdentificationType.Passport
            ? new[] { InvestmentDocumentType.IdentityFront }
            : new[] { InvestmentDocumentType.IdentityFront, InvestmentDocumentType.IdentityBack };
        var uploaded = await db.InvestmentApplicationDocuments.AsNoTracking().Where(document => document.InvestmentApplicationId == application.Id && document.IsActive)
            .Select(document => document.DocumentType).Distinct().ToListAsync();
        var missing = required.Except(uploaded).ToArray();
        return new InvestmentDocumentRequirementsDto { RequiredTypes = required, UploadedTypes = uploaded, MissingTypes = missing, IsComplete = missing.Length == 0 };
    }
    private static InvestmentApplicationDocumentDto Map(InvestmentApplicationDocument document) => new() { Id = document.Id, InvestmentApplicationId = document.InvestmentApplicationId, DocumentType = document.DocumentType, OriginalFileName = document.OriginalFileName, ContentType = document.ContentType, FileSize = document.FileSize, UploadedAt = document.UploadedAt, IsActive = document.IsActive };
}
