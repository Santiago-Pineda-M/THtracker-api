using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IActivityLogRepository
{
    Task<IEnumerable<ActivityLog>> GetAllByActivityAsync(
        Guid activityId,
        CancellationToken cancellationToken = default
    );
    Task<ActivityLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default);
    Task UpdateAsync(ActivityLog log, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActivityLog>> GetActiveLogsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActivityLog>> GetOverlappingLogsAsync(Guid userId, DateTime start, DateTime end, Guid? excludeLogId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<ActivityLog>> GetLogsAsync(
        Guid userId,
        Guid? activityId = null,
        DateTime? start = null,
        DateTime? end = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
