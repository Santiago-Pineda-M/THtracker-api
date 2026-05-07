using Microsoft.Extensions.DependencyInjection;
using THtracker.API.Security;
using THtracker.Application.Interfaces;

namespace THtracker.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IClientIpProvider, HttpContextClientIpProvider>();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        return services;
    }
}
