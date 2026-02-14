using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Roles;

public class DeleteRoleUseCase
{
    private readonly IRoleRepository _roleRepository;

    public DeleteRoleUseCase(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<bool> ExecuteAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _roleRepository.DeleteAsync(roleId, cancellationToken);
    }
}
