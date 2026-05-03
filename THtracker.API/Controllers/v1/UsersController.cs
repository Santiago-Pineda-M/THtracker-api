using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using THtracker.API.Extensions;
using THtracker.Application.Constants;
using THtracker.Application.Features.Users.Queries.GetUserById;
using THtracker.Application.Features.Users.Commands.CreateUser;
using THtracker.Application.Features.Users.Commands.UpdateUser;
using THtracker.Application.Features.Users.Commands.DeleteUser;

namespace THtracker.API.Controllers.v1;

/// <summary>
/// Gestión de usuarios: autogestión (me) y CRUD administrativo.
/// </summary>
[ApiController]
[Route("users")]
public sealed class UsersController : AuthorizedControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Obtiene los datos del usuario autenticado (autogestión).
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await _sender.Send(new GetUserByIdQuery(userId), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Actualiza los datos del usuario autenticado (autogestión).
    /// </summary>
    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        var userId = GetUserId();
        // Aseguramos que el ID del comando sea el del usuario autenticado
        var result = await _sender.Send(command with { Id = userId }, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Lista todos los usuarios (solo Admin).
    /// </summary>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        // TODO: Implement GetAllUsersQuery
        return Ok(Enumerable.Empty<UserResponse>());
    }

    /// <summary>
    /// Obtiene un usuario por su ID (solo Admin).
    /// </summary>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea un nuevo usuario (solo Admin).
    /// </summary>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }
        return result.ToActionResult();
    }

    /// <summary>
    /// Actualiza un usuario por ID (solo Admin).
    /// </summary>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command with { Id = id }, ct);
        return result.ToActionResult();
    }

    /// <summary>
    /// Elimina un usuario por ID (solo Admin).
    /// </summary>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _sender.Send(new DeleteUserCommand(id), ct);
        return result.ToActionResult();
    }
}