using THtracker.Application.DTOs.Roles;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Roles;

public class GetRoleByIdUseCase
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByIdUseCase(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<RoleResponse>> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role == null)
            return Result.Failure<RoleResponse>(new Error("NotFound", "El rol no existe."));

        return new RoleResponse(role.Id, role.Name);
    }
}
