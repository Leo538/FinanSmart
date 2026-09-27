namespace FinanSmart.Api.Modules.Investments.IdentityVerification.DTOs;
public sealed record IdentitySelfieDownload(Stream Content,string ContentType) : IDisposable { public void Dispose()=>Content.Dispose(); }
