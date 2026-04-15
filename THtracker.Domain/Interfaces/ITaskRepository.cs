namespace THtracker.Domain.Interfaces;

using THtracker.Domain.Entities;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllByTaskListAsync(Guid taskListId, CancellationToken cancellationToken = default);

    Task<IEnumerable<TaskItem>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
