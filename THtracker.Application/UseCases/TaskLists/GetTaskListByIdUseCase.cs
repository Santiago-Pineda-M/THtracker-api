namespace THtracker.Application.UseCases.TaskLists;

using THtracker.Application.DTOs.TaskLists;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

public class GetTaskListByIdUseCase
{
    private readonly ITaskListRepository taskListRepository;

    public GetTaskListByIdUseCase(ITaskListRepository taskListRepository)
    {
        this.taskListRepository = taskListRepository;
    }

    public async Task<Result<TaskListResponse>> ExecuteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var taskList = await this.taskListRepository.GetByIdAsync(id, cancellationToken);

        if (taskList == null)
        {
            return Result.Failure<TaskListResponse>(new Error("NotFound", "Task list not found"));
        }

        if (taskList.UserId != userId)
        {
            return Result.Failure<TaskListResponse>(new Error("Forbidden", "You do not own this task list"));
        }

        return new TaskListResponse(taskList.Id, taskList.UserId, taskList.Name, taskList.Color, taskList.CreatedAt, taskList.UpdatedAt);
    }
}
