namespace FinanSmart.Api.Modules.Investments.Documents;

public class DocumentValidationOptions
{
    public const string SectionName = "DocumentValidation";
    public float MinimumFieldConfidence { get; init; } = .75f;
}
