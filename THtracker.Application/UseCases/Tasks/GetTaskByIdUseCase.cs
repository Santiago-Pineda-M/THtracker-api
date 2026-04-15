namespace THtracker.Application.UseCases.Tasks;

using THtracker.Application.DTOs.Tasks;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

public class GetTaskByIdUseCase
{
    private readonly ITaskRepository taskRepository;

    public GetTaskByIdUseCase(ITaskRepository taskRepository)
    {
        this.taskRepository = taskRepository;
    }

    public async Task<Result<TaskItemResponse>> ExecuteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var task = await this.taskRepository.GetByIdAsync(id, cancellationToken);

        if (task == null)
        {
            return Result.Failure<TaskItemResponse>(new Error("NotFound", "Task not found"));
        }

        if (task.UserId != userId)
        {
            return Result.Failure<TaskItemResponse>(new Error("Forbidden", "You do not own this task"));
        }

        return new TaskItemResponse(
            task.Id,
            task.TaskListId,
            task.UserId,
            task.Content,
            task.IsCompleted,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}
