using System.Text;
using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using THtracker.Application.Interfaces;
using THtracker.Application.UseCases.Auth;
using THtracker.Application.UseCases.Users;
using THtracker.Application.UseCases.Roles;
using THtracker.Application.UseCases.Categories;
using THtracker.Application.UseCases.Activities;
using THtracker.Application.UseCases.ActivityLogs;
using THtracker.Application.UseCases.ActivityValueDefinitions;
using THtracker.Application.UseCases.ActivityLogValues;
using THtracker.Application.UseCases.Reports;
using THtracker.Application.UseCases.Seed;
using THtracker.Application;
using THtracker.Application.DTOs.Seed;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using THtracker.Infrastructure.Seeding;
using THtracker.Infrastructure.Services;
using THtracker.API.Middlewares;
using THtracker.API.Routing;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Services
// =======================

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new ApiVersioningConvention());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
);

// Authentication & Authorization
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)
            ),
        };
    });

builder.Services.AddAuthorization();

// =======================
// Clean Architecture DI
// =======================

// Domain Interfaces -> Infrastructure Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();  
builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
builder.Services.AddScoped<IActivityValueDefinitionRepository, ActivityValueDefinitionRepository>();
builder.Services.AddScoped<IActivityLogValueRepository, ActivityLogValueRepository>();


// Application Interfaces -> Infrastructure Services
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ISocialAuthenticator, DummySocialAuthenticator>();


// Application Layer - Use Cases (Users)
builder.Services.AddScoped<GetAllUsersUseCase>();
builder.Services.AddScoped<GetUserByIdUseCase>();
builder.Services.AddScoped<CreateUserUseCase>();
builder.Services.AddScoped<UpdateUserUseCase>();
builder.Services.AddScoped<DeleteUserUseCase>();

// Application Layer - Use Cases (Roles)
builder.Services.AddScoped<GetAllRolesUseCase>();
builder.Services.AddScoped<GetRoleByIdUseCase>();
builder.Services.AddScoped<GetRoleByNameUseCase>();
builder.Services.AddScoped<CreateRoleUseCase>();
builder.Services.AddScoped<DeleteRoleUseCase>();
builder.Services.AddScoped<GetUserRolesUseCase>();
builder.Services.AddScoped<AddRoleToUserUseCase>();
builder.Services.AddScoped<RemoveRoleFromUserUseCase>();

// Application Layer - Use Cases (Categories)
builder.Services.AddScoped<GetAllCategoriesUseCase>();
builder.Services.AddScoped<GetCategoryByIdUseCase>();
builder.Services.AddScoped<CreateCategoryUseCase>();
builder.Services.AddScoped<UpdateCategoryUseCase>();
builder.Services.AddScoped<DeleteCategoryUseCase>();

// Application Layer - Use Cases (Activities)
builder.Services.AddScoped<GetAllActivitiesUseCase>();
builder.Services.AddScoped<GetActivityByIdUseCase>();
builder.Services.AddScoped<CreateActivityUseCase>();
builder.Services.AddScoped<UpdateActivityUseCase>();
builder.Services.AddScoped<DeleteActivityUseCase>();

// Application Layer - Use Cases (Activity Logs)
builder.Services.AddScoped<GetActivityLogsByActivityUseCase>();
builder.Services.AddScoped<GetActivityLogByIdUseCase>();
builder.Services.AddScoped<StartActivityUseCase>();
builder.Services.AddScoped<StopActivityUseCase>();
builder.Services.AddScoped<UpdateActivityLogUseCase>();

// Application Layer - Use Cases (Value Definitions)
builder.Services.AddScoped<CreateValueDefinitionUseCase>();
builder.Services.AddScoped<GetValueDefinitionsUseCase>();
builder.Services.AddScoped<GetValueDefinitionByIdUseCase>();

// Application Layer - Use Cases (Log Values)
builder.Services.AddScoped<SaveLogValuesUseCase>();
builder.Services.AddScoped<GetLogValuesUseCase>();

// Application Layer - Use Cases (Reports)
builder.Services.AddScoped<GetUserActivityReportUseCase>();

// Application Layer - Use Cases (Auth)
builder.Services.AddScoped<RegisterUserUseCase>();
builder.Services.AddScoped<LoginUserUseCase>();
builder.Services.AddScoped<RefreshTokenUseCase>();
builder.Services.AddScoped<SocialLoginUseCase>();

// Application Layer - Use Cases (Seed) - puerto IDataSeeder implementado en Infrastructure
builder.Services.AddScoped<IDataSeeder, DataSeeder>();
builder.Services.AddScoped<SeedDefaultDataUseCase>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<IApplicationAssemblyMarker>();
builder.Services.AddFluentValidationAutoValidation();

// =======================
// App
// =======================

var app = builder.Build();

// =======================
// Middleware
// =======================

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed por defecto (Clean Architecture: Presentation solo invoca UseCase; persistencia vía IDataSeeder en Infrastructure)
using (var scope = app.Services.CreateScope())
{
    var seedUseCase = scope.ServiceProvider.GetRequiredService<SeedDefaultDataUseCase>();
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var input = new SeedDefaultDataInput(
        config["Seed:DefaultAdminEmail"] ?? "",
        config["Seed:DefaultAdminPassword"] ?? "",
        config["Seed:DefaultAdminName"] ?? "Administrator"
    );
    await seedUseCase.ExecuteAsync(input);
}

app.Run();
