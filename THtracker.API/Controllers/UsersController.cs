using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.Application.DTOs.Users;
using THtracker.Application.UseCases.Users;

namespace THtracker.API.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
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
    /// Obtiene la información del usuario autenticado (autogestión).
    /// </summary>
    /// <returns>Información del usuario actual.</returns>
    /// <response code="200">Retorna los datos del usuario.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !Guid.TryParse(userId, out var guid))
            return Unauthorized();
        var user = await _getUserById.ExecuteAsync(guid);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Actualiza la información del usuario autenticado (autogestión).
    /// </summary>
    /// <param name="request">Nuevos datos del usuario.</param>
    /// <response code="200">Usuario actualizado con éxito.</response>
    /// <response code="401">No autorizado.</response>
    /// <response code="404">Usuario no encontrado.</response>
    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !Guid.TryParse(userId, out var guid))
            return Unauthorized();
        var user = await _updateUser.ExecuteAsync(guid, request);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Obtiene la lista de todos los usuarios registrados (Solo Administradores).
    /// </summary>
    [Authorize(Roles = "Administrador")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get()
    {
        var users = await _getAllUsers.ExecuteAsync();
        return Ok(users);
    }

    /// <summary>
    /// Obtiene un usuario específico por su ID (Solo Administradores).
    /// </summary>
    /// <param name="id">ID único del usuario.</param>
    [Authorize(Roles = "Administrador")]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _getUserById.ExecuteAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Crea un nuevo usuario manualmente (Solo Administradores).
    /// </summary>
    /// <param name="request">Datos para la creación del usuario.</param>
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var user = await _createUser.ExecuteAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>
    /// Actualiza un usuario existente (Solo Administradores).
    /// </summary>
    /// <param name="id">ID del usuario a actualizar.</param>
    /// <param name="request">Nuevos datos para el usuario.</param>
    [Authorize(Roles = "Administrador")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var user = await _updateUser.ExecuteAsync(id, request);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Elimina un usuario del sistema (Solo Administradores).
    /// </summary>
    /// <param name="id">ID del usuario a eliminar.</param>
    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _deleteUser.ExecuteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
