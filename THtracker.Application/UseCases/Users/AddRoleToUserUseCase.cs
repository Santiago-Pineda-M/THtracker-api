using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class AddRoleToUserUseCase
{
    private readonly IUserRoleRepository _userRoleRepository;

    public AddRoleToUserUseCase(IUserRoleRepository userRoleRepository)
    {
        _userRoleRepository = userRoleRepository;
    }

    public async Task ExecuteAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        if (await _userRoleRepository.IsRoleAssignedAsync(userId, roleId, cancellationToken))
        {
            throw new Exception("Role is already assigned to this user.");
        }

        await _userRoleRepository.AddRoleToUserAsync(userId, roleId, cancellationToken);
    }
}
