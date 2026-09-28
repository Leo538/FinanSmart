using FinanSmart.Api.Common.Enums;
namespace FinanSmart.Api.Modules.Investments.Products.DTOs;
public class UpdateInvestmentProductDto { public string Name {get;init;}=string.Empty; public string? Description {get;init;} public decimal? MinimumAmount {get;init;} public decimal? MaximumAmount {get;init;} public int? MinimumTermDays {get;init;} public int? MaximumTermDays {get;init;} public InterestCalculationMethod InterestCalculationMethod {get;init;} public InterestPaymentFrequency InterestPaymentFrequency {get;init;} public bool IsActive {get;init;} }
