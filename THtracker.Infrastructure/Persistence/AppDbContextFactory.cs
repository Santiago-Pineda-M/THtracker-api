using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using DotNetEnv;

namespace THtracker.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // El comando 'dotnet ef' usualmente se ejecuta desde el directorio de la solución o del proyecto.
        // Buscamos el archivo .env subiendo niveles si es necesario hasta encontrar la carpeta THtracker.API
        var currentDir = Directory.GetCurrentDirectory();
        
        // Intentar encontrar la ruta de THtracker.API
        string apiDir = Path.Combine(currentDir, "THtracker.API");
        if (!Directory.Exists(apiDir))
        {
            apiDir = Path.Combine(currentDir, "..", "THtracker.API");
        }

        var envPath = Path.Combine(apiDir, ".env");
        if (File.Exists(envPath))
        {
            Env.Load(envPath);
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(apiDir)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = configuration.GetConnectionString("Default");

        if (string.IsNullOrEmpty(connectionString))
        {
             throw new Exception($"Connection string 'Default' not found. Search path was: {apiDir}");
        }

        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
