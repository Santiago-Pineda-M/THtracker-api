using THtracker.Domain.Common;
using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IActivityValueDefinitionRepository
{
    Task<PagedList<ActivityValueDefinition>> GetPageByActivityAsync(
        Guid activityId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ActivityValueDefinition>> GetAllByActivityAsync(
        Guid activityId,
        CancellationToken cancellationToken = default
    );
    Task<ActivityValueDefinition?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default
    );
    Task AddAsync(
        ActivityValueDefinition valueDefinition,
        CancellationToken cancellationToken = default
    );
    Task UpdateAsync(
        ActivityValueDefinition valueDefinition,
        CancellationToken cancellationToken = default
    );
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
