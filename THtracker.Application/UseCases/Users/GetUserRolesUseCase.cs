using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class GetUserRolesUseCase
{
    private readonly IUserRoleRepository _userRoleRepository;

    public GetUserRolesUseCase(IUserRoleRepository userRoleRepository)
    {
        _userRoleRepository = userRoleRepository;
    }

    public async Task<IEnumerable<Role>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _userRoleRepository.GetRolesByUserAsync(userId, cancellationToken);
    }
}
