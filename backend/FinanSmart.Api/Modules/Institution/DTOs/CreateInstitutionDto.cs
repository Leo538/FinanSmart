namespace FinanSmart.Api.Modules.Institution.DTOs;

public class CreateInstitutionDto
{
    public string Name { get; init; } = string.Empty;
    public string Ruc { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? BackgroundColor { get; init; }
    public bool IsActive { get; init; }
}
