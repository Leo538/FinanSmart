using FinanSmart.Api.Modules.Credits.CreditCharges.Interfaces;
using FinanSmart.Api.Modules.Credits.CreditCharges.Services;
using FinanSmart.Api.Modules.Credits.CreditRates.Interfaces;
using FinanSmart.Api.Modules.Credits.CreditRates.Services;
using FinanSmart.Api.Modules.Credits.CreditTypes.Interfaces;
using FinanSmart.Api.Modules.Credits.CreditTypes.Services;
using FinanSmart.Api.Modules.Credits.Simulations.Calculators;
using FinanSmart.Api.Modules.Credits.Simulations.Interfaces;
using FinanSmart.Api.Modules.Credits.Simulations.Services;
using FinanSmart.Api.Modules.Credits.Simulations.Pdf;
using FinanSmart.Api.Modules.Institution.Interfaces;
using FinanSmart.Api.Modules.Institution.Services;
using FinanSmart.Api.Modules.Investments.Products.Interfaces;
using FinanSmart.Api.Modules.Investments.Products.Services;
using FinanSmart.Api.Modules.Investments.Rates.Interfaces;
using FinanSmart.Api.Modules.Investments.Rates.Services;
using FinanSmart.Api.Modules.Investments.Simulations.Interfaces;
using FinanSmart.Api.Modules.Investments.Simulations.Services;
using FinanSmart.Api.Modules.Investments.Applications.Interfaces;
using FinanSmart.Api.Modules.Investments.Applications.Services;
using FinanSmart.Api.Modules.Investments.Documents.Interfaces;
using FinanSmart.Api.Modules.Investments.Documents.Services;
using FinanSmart.Api.Modules.Investments.IdentityVerification.Interfaces;
using FinanSmart.Api.Modules.Investments.IdentityVerification.Services;
using FinanSmart.Api.Modules.Admin.Dashboard.Interfaces;
using FinanSmart.Api.Modules.Admin.Dashboard.Services;
using FinanSmart.Api.Entities;
using FinanSmart.Api.Modules.Auth.Interfaces;
using FinanSmart.Api.Modules.Auth.Services;
using Microsoft.AspNetCore.Identity;
using Azure;
using Azure.AI.DocumentIntelligence;
using FinanSmart.Api.Modules.Investments.Documents;

namespace FinanSmart.Api.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICreditChargeService, CreditChargeService>();
        services.AddScoped<ICreditRateService, CreditRateService>();
        services.AddScoped<ICreditTypeService, CreditTypeService>();
        services.AddScoped<IAmortizationCalculator, FrenchAmortizationCalculator>();
        services.AddScoped<IAmortizationCalculator, GermanAmortizationCalculator>();
        services.AddScoped<CreditChargeCalculator>();
        services.AddScoped<ICreditComparisonService, CreditComparisonService>();
        services.AddScoped<ICreditSimulationService, CreditSimulationService>();
        services.AddScoped<ICreditSimulationPdfService, CreditSimulationPdfService>();
        services.AddScoped<IInstitutionService, InstitutionService>();
        services.AddScoped<IInvestmentProductService, InvestmentProductService>();
        services.AddScoped<IInvestmentRateService, InvestmentRateService>();
        services.AddScoped<IInvestmentSimulationService, InvestmentSimulationService>();
        services.AddScoped<IInvestmentApplicationService, InvestmentApplicationService>();
        services.AddScoped<IInvestmentApplicationDocumentService, InvestmentApplicationDocumentService>();
        services.Configure<DocumentValidationOptions>(configuration.GetSection(DocumentValidationOptions.SectionName));
        services.AddSingleton(sp =>
        {
            var endpoint = configuration["AzureDocumentIntelligence:Endpoint"] ?? throw new InvalidOperationException("Azure Document Intelligence endpoint is not configured.");
            var key = configuration["AzureDocumentIntelligence:Key"] ?? throw new InvalidOperationException("Azure Document Intelligence key is not configured.");
            return new DocumentIntelligenceClient(new Uri(endpoint), new AzureKeyCredential(key));
        });
        services.AddScoped<IAzureDocumentValidationService, AzureDocumentValidationService>();
        services.AddScoped<IInvestmentIdentityVerificationService, InvestmentIdentityVerificationService>();
        services.AddScoped<IAdminDashboardService, AdminDashboardService>();
        services.AddScoped<PasswordHasher<User>>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}
