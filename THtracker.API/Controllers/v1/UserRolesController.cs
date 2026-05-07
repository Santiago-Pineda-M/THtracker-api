using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Common;
using THtracker.Application.Constants;
using THtracker.Application.Features.Roles;
using THtracker.Application.Features.UserRoles.Commands.AddRoleToUser;
using THtracker.Application.Features.UserRoles.Commands.RemoveRoleFromUser;
using THtracker.Application.Features.UserRoles.Commands.SetUserRoles;
using THtracker.Application.Features.UserRoles.Queries.GetUserRoles;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Asignación de roles a usuarios (solo Admin).
/// </summary>
[Authorize(Roles = DefaultRoles.Admin)]
[ApiController]
[Route("users/{userId:guid}/roles")]
public sealed class UserRolesController : ControllerBase
{
    private readonly ISender _sender;

    public UserRolesController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Obtiene los roles asignados a un usuario.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = Pagination.DefaultPageSize,
        CancellationToken ct = default)
    {
        var result = await _sender.Send(new GetUserRolesQuery(userId, pageNumber, pageSize), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Asigna un rol a un usuario.
    /// </summary>
    [HttpPost("{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddRole(Guid userId, Guid roleId, CancellationToken ct)
    {
        var result = await _sender.Send(new AddRoleToUserCommand(userId, roleId), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Quita un rol de un usuario.
    /// </summary>
    [HttpDelete("{roleId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId, CancellationToken ct)
    {
        var result = await _sender.Send(new RemoveRoleFromUserCommand(userId, roleId), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Reemplaza todos los roles del usuario por la lista indicada (por nombre).
    /// </summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetRoles(Guid userId, [FromBody] SetUserRolesCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { UserId = userId }, ct);
        return result.ToActionResult();
    }
}
