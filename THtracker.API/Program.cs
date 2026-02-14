using System.Text;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
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
using THtracker.Application.Validators.Users;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;
using THtracker.Infrastructure.Repositories;
using THtracker.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Services
// =======================

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "THtracker API", Version = "v1" });

    // Define the Bearer Auth scheme
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Introduce el token JWT de esta manera: Bearer {tu_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, Array.Empty<string>() }
    });

    // XML Comments
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

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
builder.Services.AddScoped<GetRoleByNameUseCase>();
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
builder.Services.AddScoped<StartActivityUseCase>();
builder.Services.AddScoped<StopActivityUseCase>();
builder.Services.AddScoped<UpdateActivityLogUseCase>();

// Application Layer - Use Cases (Value Definitions)
builder.Services.AddScoped<CreateValueDefinitionUseCase>();
builder.Services.AddScoped<GetValueDefinitionsUseCase>();

// Application Layer - Use Cases (Log Values)
builder.Services.AddScoped<SaveLogValuesUseCase>();

// builder.Services.AddScoped<GetAllActivityLogsUseCase>();
// builder.Services.AddScoped<GetActivityLogByIdUseCase>();
// builder.Services.AddScoped<DeleteActivityLogUseCase>();

// Application Layer - Use Cases (Auth)
builder.Services.AddScoped<RegisterUserUseCase>();
builder.Services.AddScoped<LoginUserUseCase>();
builder.Services.AddScoped<RefreshTokenUseCase>();
builder.Services.AddScoped<SocialLoginUseCase>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// =======================
// App
// =======================

var app = builder.Build();

// =======================
// Middleware
// =======================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
