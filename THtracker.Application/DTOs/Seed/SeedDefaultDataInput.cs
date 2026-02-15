namespace THtracker.Application.DTOs.Seed;

/// <summary>
/// Entrada para el caso de uso de seed de datos por defecto.
/// </summary>
public record SeedDefaultDataInput(
    string DefaultAdminEmail,
    string DefaultAdminPassword,
    string DefaultAdminName
);
