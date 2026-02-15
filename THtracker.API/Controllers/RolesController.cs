using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
using THtracker.Application.Constants;
using THtracker.Application.DTOs.Roles;
using THtracker.Application.UseCases.Roles;

namespace THtracker.API.Controllers;

/// <summary>
/// Gestión de roles del sistema (solo Admin).
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
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _getAllRoles.ExecuteAsync(cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene un rol por su ID.
    /// </summary>
    /// <param name="id">ID único del rol.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _getRoleById.ExecuteAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene un rol por su nombre (conveniencia).
    /// </summary>
    /// <param name="name">Nombre del rol (ej. Admin, User).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet("by-name/{name}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByName(string name, CancellationToken cancellationToken)
    {
        var result = await _getRoleByName.ExecuteAsync(name, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea un nuevo rol.
    /// </summary>
    /// <param name="request">Nombre del rol.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _createRole.ExecuteAsync(request.Name, cancellationToken);
        
        if (result.IsSuccess)
        {
            var roleId = result.Value;
            var roleResult = await _getRoleById.ExecuteAsync(roleId, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = roleId }, roleResult.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Elimina un rol por ID.
    /// </summary>
    /// <param name="id">ID del rol a eliminar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _deleteRole.ExecuteAsync(id, cancellationToken);
        return result.ToActionResult();
    }
}
