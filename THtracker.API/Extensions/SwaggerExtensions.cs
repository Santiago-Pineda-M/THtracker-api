using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text.RegularExpressions;

namespace THtracker.API.Extensions;

/// <summary>
/// Extensiones para la configuración de Swagger y su automatización.
/// </summary>
public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            // 1. Detectar versiones dinámicamente desde los namespaces
            var versions = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ControllerBase).IsAssignableFrom(p) && !p.IsAbstract)
                .Select(t => Regex.Match(t.Namespace ?? "", @"\.Controllers\.(v\d+)", RegexOptions.IgnoreCase))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value.ToLower())
                .Distinct()
                .ToList();

            if (!versions.Any()) versions.Add("v1");

            foreach (var version in versions)
            {
                c.SwaggerDoc(version, new OpenApiInfo 
                { 
                    Title = $"THtracker API {version.ToUpper()}", 
                    Version = version 
                });
            }

            // 2. Agrupar por versión basada en namespace
            c.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;

                var ns = methodInfo.DeclaringType?.Namespace;
                if (string.IsNullOrEmpty(ns)) return docName == "v1";

                var match = Regex.Match(ns, @"\.Controllers\.(v\d+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value.ToLower() == docName;
                }

                return docName == "v1";
            });

            // 3. Configurar Seguridad (JWT)
            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Introduce el token JWT de esta manera: {tu_token}",
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

            // 4. Comentarios XML
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            var dynamicVersions = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(ControllerBase).IsAssignableFrom(p) && !p.IsAbstract)
                .Select(t => Regex.Match(t.Namespace ?? "", @"\.Controllers\.(v\d+)", RegexOptions.IgnoreCase))
                .Where(m => m.Success)
                .Select(m => m.Groups[1].Value.ToLower())
                .Distinct()
                .OrderBy(v => v);

            if (!dynamicVersions.Any())
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "THtracker API V1");
            }
            else
            {
                foreach (var version in dynamicVersions)
                {
                    c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"THtracker API {version.ToUpper()}");
                }
            }
        });

        return app;
    }
}
