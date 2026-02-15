using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.DTOs;
using THtracker.Application.Constants;
using THtracker.Application.DTOs.Roles;
using THtracker.Application.UseCases.Roles;
using THtracker.Domain.Entities;

namespace THtracker.API.Controllers;

/// <summary>
/// Gestión de roles del sistema (solo Admin). Recursos identificados por ID; conveniencia por nombre en by-name.
/// </summary>
[ApiController]
[Route("api/v1/roles")]
public class RolesController : ControllerBase
{
    private readonly GetAllRolesUseCase _getAllRoles;
    private readonly GetRoleByIdUseCase _getRoleById;
    private readonly GetRoleByNameUseCase _getRoleByName;
    private readonly CreateRoleUseCase _createRole;
    private readonly DeleteRoleUseCase _deleteRole;

    public RolesController(
        GetAllRolesUseCase getAllRoles,
        GetRoleByIdUseCase getRoleById,
        GetRoleByNameUseCase getRoleByName,
        CreateRoleUseCase createRole,
        DeleteRoleUseCase deleteRole
    )
    {
        _getAllRoles = getAllRoles;
        _getRoleById = getRoleById;
        _getRoleByName = getRoleByName;
        _createRole = createRole;
        _deleteRole = deleteRole;
    }

    /// <summary>
    /// Lista todos los roles.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de roles (Id, Name).</returns>
    /// <response code="200">Lista de roles.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">Sin rol Admin.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var roles = await _getAllRoles.ExecuteAsync(cancellationToken);
        var response = roles.Select(r => new RoleResponse(r.Id, r.Name));
        return Ok(response);
    }

    /// <summary>
    /// Obtiene un rol por su ID.
    /// </summary>
    /// <param name="id">ID único del rol.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Rol encontrado.</returns>
    /// <response code="200">Rol.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">Sin rol Admin.</response>
    /// <response code="404">Rol no encontrado.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var role = await _getRoleById.ExecuteAsync(id, cancellationToken);
        return role == null ? NotFound() : Ok(role);
    }

    /// <summary>
    /// Obtiene un rol por su nombre (conveniencia).
    /// </summary>
    /// <param name="name">Nombre del rol (ej. Admin, User).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Rol encontrado.</returns>
    /// <response code="200">Rol.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">Sin rol Admin.</response>
    /// <response code="404">Rol no encontrado.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet("by-name/{name}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken)
    {
        var roleEntity = await _getRoleByName.ExecuteAsync(name, cancellationToken);
        if (roleEntity == null)
            return NotFound();
        return Ok(new RoleResponse(roleEntity.Id, roleEntity.Name));
    }

    /// <summary>
    /// Crea un nuevo rol.
    /// </summary>
    /// <param name="request">Nombre del rol.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Rol creado y cabecera Location a GET por ID.</returns>
    /// <response code="201">Rol creado.</response>
    /// <response code="400">Nombre duplicado o inválido.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">Sin rol Admin.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var roleId = await _createRole.ExecuteAsync(request.Name, cancellationToken);
            var role = await _getRoleById.ExecuteAsync(roleId, cancellationToken);
            if (role == null)
                return StatusCode(500, new ApiErrorResponse("Rol creado pero no encontrado."));
            return CreatedAtAction(nameof(GetById), new { id = roleId }, role);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Elimina un rol por ID.
    /// </summary>
    /// <param name="id">ID del rol a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <response code="204">Eliminado correctamente.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">Sin rol Admin.</response>
    /// <response code="404">Rol no encontrado.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _deleteRole.ExecuteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
