using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.API.Extensions;
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
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserId();
        var result = await _getUserById.ExecuteAsync(userId);
        return result.ToActionResult();
    }

    /// <summary>
    /// Actualiza los datos del usuario autenticado (autogestión).
    /// </summary>
    /// <param name="request">Nombre y/o email nuevos.</param>
    [Authorize]
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserRequest request)
    {
        var userId = GetUserId();
        var result = await _updateUser.ExecuteAsync(userId, request);
        return result.ToActionResult();
    }

    /// <summary>
    /// Lista todos los usuarios (solo Admin).
    /// </summary>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get()
    {
        var result = await _getAllUsers.ExecuteAsync();
        return result.ToActionResult();
    }

    /// <summary>
    /// Obtiene un usuario por su ID (solo Admin).
    /// </summary>
    /// <param name="id">ID único del usuario.</param>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _getUserById.ExecuteAsync(id);
        return result.ToActionResult();
    }

    /// <summary>
    /// Crea un nuevo usuario (solo Admin).
    /// </summary>
    /// <param name="request">Nombre, email y contraseña.</param>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var result = await _createUser.ExecuteAsync(request);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
        }

        return result.ToActionResult();
    }

    /// <summary>
    /// Actualiza un usuario por ID (solo Admin).
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="request">Nombre y/o email nuevos.</param>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
    {
        var result = await _updateUser.ExecuteAsync(id, request);
        return result.ToActionResult();
    }

    /// <summary>
    /// Elimina un usuario por ID (solo Admin).
    /// </summary>
    /// <param name="id">ID del usuario a eliminar.</param>
    [Authorize(Roles = DefaultRoles.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _deleteUser.ExecuteAsync(id);
        return result.ToActionResult();
    }
}
