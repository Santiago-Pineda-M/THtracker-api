namespace THtracker.Application.UseCases.TaskLists;

using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

public class DeleteTaskListUseCase
{
    private readonly ITaskListRepository taskListRepository;
    private readonly ITaskRepository taskRepository;
    private readonly IUnitOfWork unitOfWork;

    public DeleteTaskListUseCase(
        ITaskListRepository taskListRepository,
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork
    )
    {
        this.taskListRepository = taskListRepository;
        this.taskRepository = taskRepository;
        this.unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> ExecuteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var taskList = await this.taskListRepository.GetByIdAsync(id, cancellationToken);

        if (taskList == null)
        {
            return Result.Failure<bool>(new Error("NotFound", "Task list not found"));
        }

        if (taskList.UserId != userId)
        {
            return Result.Failure<bool>(new Error("Forbidden", "You do not own this task list"));
        }

        var tasks = await this.taskRepository.GetAllByTaskListAsync(id, cancellationToken);
        foreach (var task in tasks)
        {
            await this.taskRepository.DeleteAsync(task.Id, cancellationToken);
        }

        await this.taskListRepository.DeleteAsync(id, cancellationToken);
        await this.unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
