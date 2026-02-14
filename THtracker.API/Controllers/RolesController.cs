using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using THtracker.Application.DTOs.Roles;
using THtracker.Application.UseCases.Roles;
using THtracker.Domain.Entities;

namespace THtracker.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
public class RolesController : ControllerBase
{
    private readonly GetAllRolesUseCase _getAllRoles;
    private readonly CreateRoleUseCase _createRole;
    private readonly DeleteRoleUseCase _deleteRole;
    private readonly GetRoleByNameUseCase _getRoleByName;

    public RolesController(
        GetAllRolesUseCase getAllRoles,
        CreateRoleUseCase createRole,
        DeleteRoleUseCase deleteRole,
        GetRoleByNameUseCase getRoleByName
    )
    {
        _getAllRoles = getAllRoles;
        _createRole = createRole;
        _deleteRole = deleteRole;
        _getRoleByName = getRoleByName;
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles = await _getAllRoles.ExecuteAsync();
        return Ok(roles);
    }

    [Authorize(Roles = "Administrador")]
    [HttpGet("{name}")]
    public async Task<IActionResult> GetByName(string name)
    {
        var role = await _getRoleByName.ExecuteAsync(name);
        return role is null ? NotFound() : Ok(role);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var roleId = await _createRole.ExecuteAsync(request.Name);
        return CreatedAtAction(nameof(GetByName), new { name = request.Name }, roleId);
    }

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _deleteRole.ExecuteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
