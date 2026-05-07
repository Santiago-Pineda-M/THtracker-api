using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Common;
using THtracker.Application.Constants;
using THtracker.Application.Features.Roles;
using THtracker.Application.Features.Roles.Commands.CreateRole;
using THtracker.Application.Features.Roles.Commands.DeleteRole;
using THtracker.Application.Features.Roles.Queries.GetAllRoles;
using THtracker.Application.Features.Roles.Queries.GetRoleById;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Gestión de roles del sistema (solo Admin).
/// </summary>
[Authorize(Roles = DefaultRoles.Admin)]
[ApiController]
[Route("roles")]
public sealed class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Lista todos los roles.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetAllRolesQuery(pageNumber, pageSize), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene un rol por su ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetRoleByIdQuery(id), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea un nuevo rol.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Elimina un rol por ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteRoleCommand(id), ct);
        return result.ToActionResult();
    }
}
