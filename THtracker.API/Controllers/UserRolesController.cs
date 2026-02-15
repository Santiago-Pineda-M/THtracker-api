using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.DTOs;
using THtracker.Application.Constants;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Roles;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.Entities;

namespace THtracker.API.Controllers;

/// <summary>
/// Asignación de roles a usuarios (solo Admin).
/// </summary>
[ApiController]
[Route("api/v1/users/{userId:guid}/roles")]
public class UserRolesController : ControllerBase
{
    private readonly GetUserRolesUseCase _getUserRoles;
    private readonly AddRoleToUserUseCase _addRoleToUser;
    private readonly RemoveRoleFromUserUseCase _removeRoleFromUser;
    private readonly GetRoleByNameUseCase _getRoleByName;

    public UserRolesController(
        GetUserRolesUseCase getUserRoles,
        AddRoleToUserUseCase addRoleToUser,
        RemoveRoleFromUserUseCase removeRoleFromUser,
        GetRoleByNameUseCase getRoleByName
    )
    {
        _getUserRoles = getUserRoles;
        _addRoleToUser = addRoleToUser;
        _removeRoleFromUser = removeRoleFromUser;
        _getRoleByName = getRoleByName;
    }

    /// <summary>
    /// Obtiene los roles asignados a un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de roles del usuario.</returns>
    /// <response code="200">Lista de roles.</response>
    /// <response code="403">Sin rol Admin.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Role>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoles(Guid userId, CancellationToken cancellationToken)
    {
        var roles = await _getUserRoles.ExecuteAsync(userId, cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Asigna un rol a un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="roleId">ID del rol a asignar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Rol asignado.</response>
    /// <response code="400">El rol ya estaba asignado.</response>
    /// <response code="403">Sin rol Admin.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpPost("{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddRole(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        try
        {
            await _addRoleToUser.ExecuteAsync(userId, roleId, cancellationToken);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Quita un rol de un usuario.
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="roleId">ID del rol a quitar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Rol quitado.</response>
    /// <response code="403">Sin rol Admin.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpDelete("{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        await _removeRoleFromUser.ExecuteAsync(userId, roleId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Reemplaza todos los roles del usuario por la lista indicada (por nombre).
    /// </summary>
    /// <param name="userId">ID del usuario.</param>
    /// <param name="request">Nombres de los roles a asignar (reemplaza la lista actual).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Roles actualizados.</response>
    /// <response code="403">Sin rol Admin.</response>
    /// <response code="404">Algún nombre de rol no existe.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetRoles(
        Guid userId,
        [FromBody] SetUserRolesRequest request,
        CancellationToken cancellationToken
    )
    {
        var currentRoles = await _getUserRoles.ExecuteAsync(userId, cancellationToken);
        foreach (var r in currentRoles)
        {
            await _removeRoleFromUser.ExecuteAsync(userId, r.Id, cancellationToken);
        }

        if (request.RoleNames is { Count: > 0 })
        {
            foreach (var roleName in request.RoleNames)
            {
                var roleEntity = await _getRoleByName.ExecuteAsync(roleName, cancellationToken);
                if (roleEntity == null)
                    return NotFound(new ApiErrorResponse($"Rol '{roleName}' no encontrado."));

                await _addRoleToUser.ExecuteAsync(userId, roleEntity.Id, cancellationToken);
            }
        }

        return NoContent();
    }
}
