using FinanSmart.Api.Common.Enums;

namespace FinanSmart.Api.Modules.Investments.Applications.DTOs;

public class UpdateInvestmentApplicantDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public IdentificationType IdentificationType { get; init; }
    public string IdentificationNumber { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public DateOnly? BirthDate { get; init; }
    public string Address { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
}
