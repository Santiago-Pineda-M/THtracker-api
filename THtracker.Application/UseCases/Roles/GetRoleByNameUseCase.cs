using THtracker.Application.DTOs.Roles;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Roles;

public class GetRoleByNameUseCase
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByNameUseCase(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<RoleResponse>> ExecuteAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        var role = await _roleRepository.GetByNameAsync(name, cancellationToken);
        if (role == null)
            return Result.Failure<RoleResponse>(new Error("NotFound", $"El rol '{name}' no existe."));

        return new RoleResponse(role.Id, role.Name);
    }
}
