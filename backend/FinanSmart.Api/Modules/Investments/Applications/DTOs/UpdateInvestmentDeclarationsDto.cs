using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Investments.Applications.DTOs;

public class UpdateInvestmentDeclarationsDto
{
    public SourceOfFunds? SourceOfFunds { get; init; }
    public string? OtherSourceOfFunds { get; init; }
    public bool InformationAccuracyAccepted { get; init; }
    public bool TermsAccepted { get; init; }
    public bool DataProcessingAccepted { get; init; }
}
