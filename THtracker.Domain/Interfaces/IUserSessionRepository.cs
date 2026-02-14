using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IUserSessionRepository
{
    Task<IEnumerable<UserSession>> GetAllByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<UserSession?> GetByTokenAsync(
        string sessionToken,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(UserSession session, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserSession session, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
