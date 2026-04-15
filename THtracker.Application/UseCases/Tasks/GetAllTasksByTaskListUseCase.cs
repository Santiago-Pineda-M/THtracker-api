namespace THtracker.Application.UseCases.Tasks;

using THtracker.Application.DTOs.Tasks;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

public class GetAllTasksByTaskListUseCase
{
    private readonly ITaskRepository taskRepository;
    private readonly ITaskListRepository taskListRepository;

    public GetAllTasksByTaskListUseCase(ITaskRepository taskRepository, ITaskListRepository taskListRepository)
    {
        this.taskRepository = taskRepository;
        this.taskListRepository = taskListRepository;
    }

    public async Task<Result<IEnumerable<TaskItemResponse>>> ExecuteAsync(
        Guid userId,
        Guid taskListId,
        CancellationToken cancellationToken = default
    )
    {
        var taskList = await this.taskListRepository.GetByIdAsync(taskListId, cancellationToken);

        if (taskList == null)
        {
            return Result.Failure<IEnumerable<TaskItemResponse>>(new Error("NotFound", "Task list not found"));
        }

        if (taskList.UserId != userId)
        {
            return Result.Failure<IEnumerable<TaskItemResponse>>(new Error("Forbidden", "You do not own this task list"));
        }

        var tasks = await this.taskRepository.GetAllByTaskListAsync(taskListId, cancellationToken);
        var response = tasks.Select(t => new TaskItemResponse(
            t.Id,
            t.TaskListId,
            t.UserId,
            t.Content,
            t.IsCompleted,
            t.DueDate,
            t.CreatedAt,
            t.UpdatedAt
        ));

        return Result.Success<IEnumerable<TaskItemResponse>>(response);
    }
}
