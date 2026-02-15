using THtracker.Application.DTOs.Roles;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class SetUserRolesUseCase
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SetUserRolesUseCase(
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork)
    {
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> ExecuteAsync(Guid userId, List<string> roleNames, CancellationToken cancellationToken = default)
    {
        // 1. Get current roles
        var currentRoles = await _userRoleRepository.GetRolesByUserAsync(userId, cancellationToken);
        
        // 2. Remove all current roles
        foreach (var r in currentRoles)
        {
            await _userRoleRepository.RemoveRoleFromUserAsync(userId, r.Id, cancellationToken);
        }

        // 3. Add new roles
        if (roleNames is { Count: > 0 })
        {
            foreach (var roleName in roleNames)
            {
                var role = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
                if (role == null)
                    return Result.Failure(new Error("NotFound", $"El rol '{roleName}' no existe."));

                await _userRoleRepository.AddRoleToUserAsync(userId, role.Id, cancellationToken);
            }
        }

        // 4. Save changes once
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        return Result.Success();
    }
}
