using THtracker.Application.DTOs.Seed;
using THtracker.Application.Interfaces;

namespace THtracker.Application.UseCases.Seed;

/// <summary>
/// Caso de uso: asegurar datos iniciales (roles y admin) al arranque.
/// Orquesta el puerto IDataSeeder; la persistencia la implementa Infrastructure.
/// </summary>
public class SeedDefaultDataUseCase
{
    private readonly IDataSeeder _dataSeeder;

    public SeedDefaultDataUseCase(IDataSeeder dataSeeder)
    {
        _dataSeeder = dataSeeder;
    }

    public Task ExecuteAsync(
        SeedDefaultDataInput input,
        CancellationToken cancellationToken = default
    )
    {
        if (
            string.IsNullOrWhiteSpace(input.DefaultAdminEmail)
            || string.IsNullOrWhiteSpace(input.DefaultAdminPassword)
        )
        {
            return Task.CompletedTask;
        }

        var name = string.IsNullOrWhiteSpace(input.DefaultAdminName)
            ? "Administrator"
            : input.DefaultAdminName;

        return _dataSeeder.SeedAsync(
            input.DefaultAdminEmail.Trim(),
            input.DefaultAdminPassword,
            name,
            cancellationToken
        );
    }
}
