using THtracker.Domain.Common;
using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IUserRoleRepository
{
    Task<PagedList<Role>> GetRolesPageByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Role>> GetRolesByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<User>> GetUsersByRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default
    );
    Task AddRoleToUserAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    );
    Task RemoveRoleFromUserAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    );
    Task<bool> IsRoleAssignedAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default
    );
}
