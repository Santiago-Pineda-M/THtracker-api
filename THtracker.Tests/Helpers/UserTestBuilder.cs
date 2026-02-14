using System.Reflection;
using THtracker.Domain.Entities;

namespace THtracker.Tests.Helpers;

/// <summary>
/// Builder para crear instancias de <see cref="User"/> en tests con datos controlados (ej. Id fijo).
/// Centraliza el uso de reflexión para no repetirlo en varios tests.
/// </summary>
public static class UserTestBuilder
{
    private static readonly PropertyInfo? IdProperty = typeof(User).GetProperty("Id");

    /// <summary>
    /// Crea un User con un Id específico (útil para mocks y verificaciones).
    /// </summary>
    public static User WithId(string name, string email, Guid id)
    {
        var user = new User(name, email);
        IdProperty?.SetValue(user, id);
        return user;
    }
}
