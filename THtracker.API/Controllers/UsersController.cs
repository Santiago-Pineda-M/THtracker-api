using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.DTOs;
using THtracker.Application.Constants;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;

namespace THtracker.API.Controllers;

/// <summary>
/// Gestión de usuarios: autogestión (me) y CRUD administrativo.
/// </summary>
[ApiController]
[Route("api/v1/users")]
public class UsersController : AuthorizedControllerBase
{
    private readonly GetAllUsersUseCase _getAllUsers;
    private readonly GetUserByIdUseCase _getUserById;
    private readonly CreateUserUseCase _createUser;
    private readonly UpdateUserUseCase _updateUser;
    private readonly DeleteUserUseCase _deleteUser;

    public UsersController(
        GetAllUsersUseCase getAllUsers,
        GetUserByIdUseCase getUserById,
        CreateUserUseCase createUser,
        UpdateUserUseCase updateUser,
        DeleteUserUseCase deleteUser
    )
    {
        _getAllUsers = getAllUsers;
        _getUserById = getUserById;
        _createUser = createUser;
        _updateUser = updateUser;
        _deleteUser = deleteUser;
    }

    /// <summary>
    /// Obtiene los datos del usuario autenticado (autogestión).
    /// </summary>
    /// <returns>Información del usuario actual.</returns>
    /// <response code="200">Datos del usuario.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserId();
        var user = await _getUserById.ExecuteAsync(userId);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Actualiza los datos del usuario autenticado (autogestión).
    /// </summary>
    /// <param name="request">Nombre y/o email nuevos.</param>
    /// <returns>Usuario actualizado.</returns>
    /// <response code="200">Actualización correcta.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request)
    {
        var userId = GetUserId();
        var user = await _updateUser.ExecuteAsync(userId, request);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Lista todos los usuarios (solo Admin).
    /// </summary>
    /// <returns>Lista de usuarios.</returns>
    /// <response code="200">Lista de usuarios.</response>
    /// <response code="403">Sin rol Admin.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get()
    {
        var users = await _getAllUsers.ExecuteAsync();
        return Ok(users);
    }

    /// <summary>
    /// Obtiene un usuario por su ID (solo Admin).
    /// </summary>
    /// <param name="id">ID único del usuario.</param>
    /// <returns>Usuario encontrado.</returns>
    /// <response code="200">Usuario.</response>
    /// <response code="403">Sin rol Admin.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _getUserById.ExecuteAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Crea un nuevo usuario (solo Admin).
    /// </summary>
    /// <param name="request">Nombre, email y contraseña.</param>
    /// <returns>Usuario creado y ubicación en cabecera Location.</returns>
    /// <response code="201">Usuario creado.</response>
    /// <response code="400">Datos inválidos o email duplicado.</response>
    /// <response code="403">Sin rol Admin.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _createUser.ExecuteAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Actualiza un usuario por ID (solo Admin).
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="request">Nombre y/o email nuevos.</param>
    /// <returns>Usuario actualizado.</returns>
    /// <response code="200">Actualización correcta.</response>
    /// <response code="403">Sin rol Admin.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _updateUser.ExecuteAsync(id, request);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Elimina un usuario por ID (solo Admin).
    /// </summary>
    /// <param name="id">ID del usuario a eliminar.</param>
    /// <response code="204">Eliminado correctamente.</response>
    /// <response code="403">Sin rol Admin.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _deleteUser.ExecuteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
