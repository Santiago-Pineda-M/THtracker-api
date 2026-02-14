using THtracker.Domain.Entities;

namespace THtracker.Domain.Interfaces;

public interface IActivityValueDefinitionRepository
{
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
