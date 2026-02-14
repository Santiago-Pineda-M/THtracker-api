using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Roles;

public class GetAllRolesUseCase
{
    private readonly IRoleRepository _roleRepository;

    public GetAllRolesUseCase(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<IEnumerable<Role>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return await _roleRepository.GetAllAsync(cancellationToken);
    }
}
