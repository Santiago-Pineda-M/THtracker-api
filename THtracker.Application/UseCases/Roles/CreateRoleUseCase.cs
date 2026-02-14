using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Roles;

public class CreateRoleUseCase
{
    private readonly IRoleRepository _roleRepository;

    public CreateRoleUseCase(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Guid> ExecuteAsync(string name, CancellationToken cancellationToken = default)
    {
        var existingRole = await _roleRepository.GetByNameAsync(name, cancellationToken);
        if (existingRole != null)
        {
            throw new Exception($"Role '{name}' already exists.");
        }

        var role = new Role(name);
        await _roleRepository.AddAsync(role, cancellationToken);
        return role.Id;
    }
}
