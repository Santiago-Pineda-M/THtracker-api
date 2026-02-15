using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.Constants;
using THtracker.Application.DTOs.Roles;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Asignación de roles a usuarios (solo Admin).
/// </summary>
[ApiController]
[Route("users/{userId:guid}/roles")]
[Authorize(Roles = DefaultRoles.Admin)]
public class UserRolesController : ControllerBase
{
    private readonly GetUserRolesUseCase _getUserRoles;
    private readonly AddRoleToUserUseCase _addRoleToUser;
    private readonly RemoveRoleFromUserUseCase _removeRoleFromUser;
    private readonly SetUserRolesUseCase _setUserRoles;

    public UserRolesController(
        GetUserRolesUseCase getUserRoles,
        AddRoleToUserUseCase addRoleToUser,
        RemoveRoleFromUserUseCase removeRoleFromUser,
        SetUserRolesUseCase setUserRoles
    )
    {
        _getUserRoles = getUserRoles;
        _addRoleToUser = addRoleToUser;
        _removeRoleFromUser = removeRoleFromUser;
        _setUserRoles = setUserRoles;
    }

    /// <summary>
    /// Obtiene los roles asignados a un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _getUserRoles.ExecuteAsync(userId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Asigna un rol a un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="roleId">ID del rol a asignar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPost("{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddRole(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _addRoleToUser.ExecuteAsync(userId, roleId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Quita un rol de un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="roleId">ID del rol a quitar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpDelete("{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        var result = await _removeRoleFromUser.ExecuteAsync(userId, roleId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Reemplaza todos los roles del usuario por la lista indicada (por nombre).
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="request">Nombres de los roles a asignar (reemplaza la lista actual).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetRoles(
        Guid userId,
        [FromBody] SetUserRolesRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _setUserRoles.ExecuteAsync(userId, request.RoleNames, cancellationToken);
        return result.ToActionResult();
    }
}
