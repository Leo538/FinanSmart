using FinanSmart.Api.Common.Enums;
namespace FinanSmart.Api.Entities;
public class InvestmentIdentityVerification
{
    public Guid Id { get; set; } public Guid InvestmentApplicationId { get; set; }
    public string? SelfieOriginalFileName { get; set; } public string? SelfieStoredFileName { get; set; } public string? SelfieStoragePath { get; set; }
    public string? SelfieContentType { get; set; } public long? SelfieFileSize { get; set; }
    public bool ConsentAccepted { get; set; } public IdentityVerificationStatus Status { get; set; }
    public DateTimeOffset? CapturedAt { get; set; } public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } public DateTimeOffset UpdatedAt { get; set; }
    public InvestmentApplication InvestmentApplication { get; set; } = null!;
}
