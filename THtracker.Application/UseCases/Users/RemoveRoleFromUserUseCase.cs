using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class RemoveRoleFromUserUseCase
{
    private readonly IUserRoleRepository _userRoleRepository;

    public RemoveRoleFromUserUseCase(IUserRoleRepository userRoleRepository)
    {
        _userRoleRepository = userRoleRepository;
    }

    public async Task ExecuteAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    )
    {
        await _userRoleRepository.RemoveRoleFromUserAsync(userId, roleId, cancellationToken);
    }
}
