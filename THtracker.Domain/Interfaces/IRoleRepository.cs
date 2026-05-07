using THtracker.Domain.Common;
using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IRoleRepository
{
    Task<PagedList<Role>> GetPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
