namespace THtracker.Application.DTOs.Users;

/// <summary>
/// Solicitud para reemplazar los roles de un usuario.
/// </summary>
/// <param name="RoleNames">Nombres de los roles a asignar (reemplaza la lista actual).</param>
public record SetUserRolesRequest(List<string>? RoleNames);
