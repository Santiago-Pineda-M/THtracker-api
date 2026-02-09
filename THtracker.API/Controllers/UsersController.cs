using Microsoft.AspNetCore.Mvc;
using THtracker.Application.UseCases.Users;
using THtracker.Domain.DTOs;

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
        DeleteUserUseCase deleteUser)
    {
        _getAllUsers = getAllUsers;
        _getUserById = getUserById;
        _createUser = createUser;
        _updateUser = updateUser;
        _deleteUser = deleteUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var users = await _getAllUsers.ExecuteAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _getUserById.ExecuteAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var user = await _createUser.ExecuteAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await _updateUser.ExecuteAsync(id, dto);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _deleteUser.ExecuteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
