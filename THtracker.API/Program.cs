using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using THtracker.API.Extensions;
using THtracker.API.Middlewares;
using THtracker.API.Routing;
using THtracker.Application;
using THtracker.Infrastructure;
using THtracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
// using THtracker.Application.Features.Seed; // Pendiente migración

Env.Load();

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

// CORS
var corsOrigins = builder.Configuration["CORS_ALLOWED_ORIGINS"] ?? "http://localhost:5173";
var allowedOrigins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()
    );
});

// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database");

// Temporary registration for seed use case until fully refactored
// builder.Services.AddScoped<SeedDefaultDataUseCase>();

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
app.UseRateLimiter();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Health Check Endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Database Migration
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();
        // Lógica de Seed desactivada temporalmente hasta migración a Features
        /*
        var seedUseCase = scope.ServiceProvider.GetRequiredService<SeedDefaultDataUseCase>();
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var input = new SeedDefaultDataInput(
            config["Seed:DefaultAdminEmail"] ?? "",
            config["Seed:DefaultAdminPassword"] ?? "",
            config["Seed:DefaultAdminName"] ?? "Administrator"
        );
        await seedUseCase.ExecuteAsync(input);
        */
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during DB migration.");
    }
}

app.Run();
