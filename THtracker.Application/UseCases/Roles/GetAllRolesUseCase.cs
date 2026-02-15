using THtracker.Application.DTOs.Roles;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Roles;

public class GetAllRolesUseCase
{
    private readonly IRoleRepository _roleRepository;

    public GetAllRolesUseCase(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<IEnumerable<RoleResponse>>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        return Result.Success(roles.Select(r => new RoleResponse(r.Id, r.Name)));
    }
}
