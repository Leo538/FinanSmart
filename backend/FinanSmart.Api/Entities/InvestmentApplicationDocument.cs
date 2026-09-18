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
    public InvestmentApplication InvestmentApplication { get; set; } = null!;
}
