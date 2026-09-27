using FinanSmart.Api.Common.Enums;
namespace FinanSmart.Api.Modules.Investments.IdentityVerification.DTOs;
public class InvestmentIdentityVerificationDto { public Guid Id{get;init;} public Guid InvestmentApplicationId{get;init;} public IdentityVerificationStatus Status{get;init;} public bool ConsentAccepted{get;init;} public string? SelfieContentType{get;init;} public long? SelfieFileSize{get;init;} public DateTimeOffset? CapturedAt{get;init;} public DateTimeOffset? VerifiedAt{get;init;} }
