using THtracker.Domain.Common;
using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IUserSessionRepository
{
    Task<PagedList<UserSession>> GetPageByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedList<UserSession>> GetActivePageByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<UserSession>> GetAllByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<IEnumerable<UserSession>> GetActiveByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<UserSession?> GetByTokenAsync(
        string sessionToken,
        CancellationToken cancellationToken = default
    );
    Task<UserSession?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(UserSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserSession session, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
