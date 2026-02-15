using THtracker.Application.DTOs.Roles;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.UseCases.Users;

public class GetUserRolesUseCase
{
    private readonly IUserRoleRepository _userRoleRepository;

    public GetUserRolesUseCase(IUserRoleRepository userRoleRepository)
    {
        _userRoleRepository = userRoleRepository;
    }

    public async Task<Result<IEnumerable<RoleResponse>>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var roles = await _userRoleRepository.GetRolesByUserAsync(userId, cancellationToken);
        var response = roles.Select(r => new RoleResponse(r.Id, r.Name));
        return Result.Success(response);
    }
}
