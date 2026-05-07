namespace THtracker.Domain.Interfaces;

using THtracker.Domain.Common;
using THtracker.Domain.Entities;

public interface ITaskListRepository
{
    Task<PagedList<TaskList>> GetPageByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskList>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<TaskList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskList?> GetByIdWithTasksAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TaskList taskList, CancellationToken cancellationToken = default);

    Task UpdateAsync(TaskList taskList, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
