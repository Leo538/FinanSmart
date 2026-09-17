using FinanSmart.Api.Modules.Credits.CreditRates.Interfaces;
using FinanSmart.Api.Modules.Credits.CreditRates.Services;
using FinanSmart.Api.Modules.Credits.CreditTypes.Interfaces;
using FinanSmart.Api.Modules.Credits.CreditTypes.Services;
using FinanSmart.Api.Modules.Institution.Interfaces;
using FinanSmart.Api.Modules.Institution.Services;

namespace FinanSmart.Api.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICreditRateService, CreditRateService>();
        services.AddScoped<ICreditTypeService, CreditTypeService>();
        services.AddScoped<IInstitutionService, InstitutionService>();
        return services;
    }
}
