using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IActivityRepository
{
    Task<IEnumerable<Activity>> GetAllByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<Activity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Activity activity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Activity activity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
