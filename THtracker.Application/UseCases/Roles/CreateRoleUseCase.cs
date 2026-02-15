using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Roles;

public class CreateRoleUseCase
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleUseCase(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> ExecuteAsync(string name, CancellationToken cancellationToken = default)
    {
        var existingRole = await _roleRepository.GetByNameAsync(name, cancellationToken);
        if (existingRole != null)
        {
            return Result.Failure<Guid>(new Error("Conflict", $"Role '{name}' already exists."));
        }

        var role = new Role(name);
        await _roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return role.Id;
    }
}
