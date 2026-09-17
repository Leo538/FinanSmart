using FinanSmart.Api.Modules.Credits.CreditCharges.Interfaces;
using FinanSmart.Api.Modules.Credits.CreditCharges.Services;
using FinanSmart.Api.Modules.Credits.CreditRates.Interfaces;
using FinanSmart.Api.Modules.Credits.CreditRates.Services;
using FinanSmart.Api.Modules.Credits.CreditTypes.Interfaces;
using FinanSmart.Api.Modules.Credits.CreditTypes.Services;
using FinanSmart.Api.Modules.Credits.Simulations.Calculators;
using FinanSmart.Api.Modules.Credits.Simulations.Interfaces;
using FinanSmart.Api.Modules.Credits.Simulations.Services;
using FinanSmart.Api.Modules.Institution.Interfaces;
using FinanSmart.Api.Modules.Institution.Services;

namespace FinanSmart.Api.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICreditChargeService, CreditChargeService>();
        services.AddScoped<ICreditRateService, CreditRateService>();
        services.AddScoped<ICreditTypeService, CreditTypeService>();
        services.AddScoped<IAmortizationCalculator, FrenchAmortizationCalculator>();
        services.AddScoped<IAmortizationCalculator, GermanAmortizationCalculator>();
        services.AddScoped<CreditChargeCalculator>();
        services.AddScoped<ICreditSimulationService, CreditSimulationService>();
        services.AddScoped<IInstitutionService, InstitutionService>();
        return services;
    }
}
