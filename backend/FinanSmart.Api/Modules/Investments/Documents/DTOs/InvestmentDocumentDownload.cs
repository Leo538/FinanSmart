namespace FinanSmart.Api.Modules.Investments.Documents.DTOs;

public sealed record InvestmentDocumentDownload(Stream Content, string ContentType, string OriginalFileName) : IDisposable
{
    public void Dispose() => Content.Dispose();
}
