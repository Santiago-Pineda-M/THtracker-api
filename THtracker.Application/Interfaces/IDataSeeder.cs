namespace THtracker.Application.Interfaces;

/// <summary>
/// Puerto para asegurar datos iniciales (roles por defecto y usuario administrador).
/// La implementación pertenece a Infrastructure.
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// Crea roles por defecto y el usuario administrador si no existen.
    /// Idempotente: no sobrescribe datos existentes.
    /// </summary>
    Task SeedAsync(
        string defaultAdminEmail,
        string defaultAdminPassword,
        string defaultAdminName,
        CancellationToken cancellationToken = default
    );
}
