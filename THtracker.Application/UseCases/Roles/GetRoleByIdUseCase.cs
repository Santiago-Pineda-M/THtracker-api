using THtracker.Application.DTOs.Roles;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Roles;

public class GetRoleByIdUseCase
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByIdUseCase(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponse?> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        return role == null ? null : new RoleResponse(role.Id, role.Name);
    }
}
