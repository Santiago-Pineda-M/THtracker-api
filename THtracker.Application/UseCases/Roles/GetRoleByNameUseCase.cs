using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Roles;

public class GetRoleByNameUseCase
{
    private readonly IRoleRepository _roleRepository;

    public GetRoleByNameUseCase(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Role?> ExecuteAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        return await _roleRepository.GetByNameAsync(name, cancellationToken);
    }
}
