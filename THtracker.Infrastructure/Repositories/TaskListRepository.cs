namespace THtracker.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;
using THtracker.Infrastructure.Persistence;

public class TaskListRepository : ITaskListRepository
{
    private readonly AppDbContext context;

    public TaskListRepository(AppDbContext context)
    {
        this.context = context;
    }

    public async Task<PagedList<TaskList>> GetPageByUserAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = context.TaskLists.AsNoTracking().Where(t => t.UserId == userId);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(t => t.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return new PagedList<TaskList>(items, total);
    }

    public async Task<IEnumerable<TaskList>> GetAllByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await this.context.TaskLists.Where(t => t.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<TaskList?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this.context.TaskLists.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<TaskList?> GetByIdWithTasksAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await this.context.TaskLists.Include(t => t.Tasks).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AddAsync(TaskList taskList, CancellationToken cancellationToken = default)
    {
        await this.context.TaskLists.AddAsync(taskList, cancellationToken);
    }

    public async Task UpdateAsync(TaskList taskList, CancellationToken cancellationToken = default)
    {
        this.context.TaskLists.Update(taskList);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var taskList = await this.context.TaskLists.FindAsync(new object[] { id }, cancellationToken);
        if (taskList == null)
        {
            return false;
        }

        this.context.TaskLists.Remove(taskList);
        return true;
    }
}
