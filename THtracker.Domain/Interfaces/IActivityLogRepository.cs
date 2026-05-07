using THtracker.Domain.Common;
using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IActivityLogRepository
{
    Task<PagedList<ActivityLog>> GetLogsPageForUserAsync(
        Guid userId,
        Guid? activityId,
        DateTime? from,
        DateTime? to,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedList<ActivityLog>> GetActiveLogsPageForUserAsync(
        Guid userId,
        Guid? activityId,
        DateTime? from,
        DateTime? to,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

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
    Task<IEnumerable<ActivityLog>> GetReportLogsAsync(
        Guid userId,
        DateTime start,
        DateTime end,
        List<Guid>? categoryIds = null,
        List<Guid>? activityIds = null,
        string? searchTerm = null,
        bool? onlyCompleted = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
