using THtracker.Domain.Common;
using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IActivityLogValueRepository
{
    Task<PagedList<ActivityLogValue>> GetPageByLogAsync(
        Guid activityLogId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ActivityLogValue>> GetAllByLogAsync(
        Guid activityLogId,
        CancellationToken cancellationToken = default
    );
    Task<ActivityLogValue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(ActivityLogValue logValue, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
