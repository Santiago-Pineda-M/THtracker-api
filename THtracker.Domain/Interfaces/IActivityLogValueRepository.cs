using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IActivityLogValueRepository
{
    Task<IEnumerable<ActivityLogValue>> GetAllByLogAsync(
        Guid activityLogId,
        CancellationToken cancellationToken = default
    );
    Task<ActivityLogValue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ActivityLogValue logValue, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
