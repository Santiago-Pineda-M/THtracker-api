using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Options;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using THtracker.Infrastructure.Services;
using THtracker.Application.Interfaces;

namespace THtracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default"))
        );

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IActivityRepository, ActivityRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IActivityValueDefinitionRepository, ActivityValueDefinitionRepository>();
        services.AddScoped<IActivityLogValueRepository, ActivityLogValueRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<ITaskListRepository, TaskListRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        // Services
        services.AddScoped<IJwtProvider, JwtProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ISocialAuthenticator, DummySocialAuthenticator>();

        return services;
    }
}
