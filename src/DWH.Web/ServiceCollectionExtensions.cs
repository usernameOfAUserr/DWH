using DWH.Application.Interfaces.Repositories;
using DWH.Application.Interfaces.Services;
using DWH.Application.Services;
using DWH.Infrastructure.Repositories;

namespace DWH.Web;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IImportService, ImportService>();
        
        services.AddScoped<IIlostatRepository, IlostatRepository>();
        services.AddScoped<IAiExposureRepository, AiExposureRepository>();
        services.AddScoped<IDestatisRepository, DestatisRepository>();
        services.AddScoped<IDbRepository, DbRepository>();
        services.AddHttpClient<IDestatisRepository, DestatisRepository>();
        services.AddHttpClient<IAiExposureRepository, AiExposureRepository>();
        
        return services;
    }
}