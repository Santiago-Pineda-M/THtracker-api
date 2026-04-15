namespace THtracker.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext context;

    public TaskRepository(AppDbContext context)
    {
        this.context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetAllByTaskListAsync(Guid taskListId, CancellationToken cancellationToken = default)
    {
        return await this.context.Tasks.Where(t => t.TaskListId == taskListId).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TaskItem>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await this.context.Tasks.Where(t => t.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this.context.Tasks.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task AddAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        await this.context.Tasks.AddAsync(task, cancellationToken);
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken cancellationToken = default)
    {
        this.context.Tasks.Update(task);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await this.context.Tasks.FindAsync(new object[] { id }, cancellationToken);
        if (task == null)
        {
            return false;
        }

        this.context.Tasks.Remove(task);
        return true;
    }
}
