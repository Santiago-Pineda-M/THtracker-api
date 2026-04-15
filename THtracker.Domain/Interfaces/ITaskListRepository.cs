namespace THtracker.Domain.Interfaces;

using THtracker.Domain.Entities;

public interface ITaskListRepository
{
    Task<IEnumerable<TaskList>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<TaskList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskList?> GetByIdWithTasksAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TaskList taskList, CancellationToken cancellationToken = default);

    Task UpdateAsync(TaskList taskList, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
