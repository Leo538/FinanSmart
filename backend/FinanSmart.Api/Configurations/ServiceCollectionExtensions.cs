using FinanSmart.Api.Modules.Institution.Interfaces;
using FinanSmart.Api.Modules.Institution.Services;

namespace FinanSmart.Api.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IInstitutionService, InstitutionService>();
        return services;
    }
}

