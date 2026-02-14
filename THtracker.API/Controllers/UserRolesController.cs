using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.Application.UseCases.Users;
using THtracker.Application.UseCases.Roles;
using THtracker.Domain.Entities;

namespace THtracker.API.Controllers;

[ApiController]
[Route("api/v1/users/{userId}/roles")]
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

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> GetRoles(Guid userId)
    {
        var roles = await _getUserRoles.ExecuteAsync(userId);
        return Ok(roles);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost("{roleId}")]
    public async Task<IActionResult> AddRole(Guid userId, Guid roleId)
    {
        await _addRoleToUser.ExecuteAsync(userId, roleId);
        return NoContent();
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{roleId}")]
    public async Task<IActionResult> RemoveRole(Guid userId, Guid roleId)
    {
        await _removeRoleFromUser.ExecuteAsync(userId, roleId);
        return NoContent();
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut]
    public async Task<IActionResult> SetRoles(Guid userId, [FromBody] string roleName)
    {
        // En una implementación real, esto debería estar dentro de un solo UseCase 
        // para asegurar una transacción atómica.
        
        var currentRoles = await _getUserRoles.ExecuteAsync(userId);
        foreach (var r in currentRoles)
        {
            await _removeRoleFromUser.ExecuteAsync(userId, r.Id);
        }

        var roleEntity = await _getRoleByName.ExecuteAsync(roleName);
        if (roleEntity == null)
            return NotFound($"Role '{roleName}' not found");

        await _addRoleToUser.ExecuteAsync(userId, roleEntity.Id);
        return NoContent();
    }
}
