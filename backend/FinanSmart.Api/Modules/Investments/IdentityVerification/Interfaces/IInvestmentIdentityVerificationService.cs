using FinanSmart.Api.Modules.Investments.IdentityVerification.DTOs; using Microsoft.AspNetCore.Http;
namespace FinanSmart.Api.Modules.Investments.IdentityVerification.Interfaces;
public interface IInvestmentIdentityVerificationService { Task<InvestmentIdentityVerificationDto?> GetAsync(Guid applicationId); Task<InvestmentIdentityVerificationDto> UploadSelfieAsync(Guid applicationId,IFormFile file); Task<IdentitySelfieDownload?> GetSelfieAsync(Guid applicationId); Task<InvestmentIdentityVerificationDto> VerifyAsync(Guid applicationId,bool consentAccepted); }
