namespace THtracker.Application.UseCases.Tasks;

using FluentValidation;
using THtracker.Application.DTOs.Tasks;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

public class CreateTaskItemUseCase
{
    private readonly ITaskRepository taskRepository;
    private readonly ITaskListRepository taskListRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IValidator<CreateTaskItemRequest> validator;

    public CreateTaskItemUseCase(
        ITaskRepository taskRepository,
        ITaskListRepository taskListRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateTaskItemRequest> validator
    )
    {
        this.taskRepository = taskRepository;
        this.taskListRepository = taskListRepository;
        this.unitOfWork = unitOfWork;
        this.validator = validator;
    }

    public async Task<Result<TaskItemResponse>> ExecuteAsync(
        Guid userId,
        CreateTaskItemRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await this.validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<TaskItemResponse>(new Error("Validation", errors));
        }

        var taskList = await this.taskListRepository.GetByIdAsync(request.TaskListId, cancellationToken);

        if (taskList == null)
        {
            return Result.Failure<TaskItemResponse>(new Error("NotFound", "Task list not found"));
        }

        if (taskList.UserId != userId)
        {
            return Result.Failure<TaskItemResponse>(new Error("Forbidden", "You do not own this task list"));
        }

        var task = new TaskItem(request.TaskListId, userId, request.Content, request.DueDate);

        await this.taskRepository.AddAsync(task, cancellationToken);
        await this.unitOfWork.SaveChangesAsync(cancellationToken);

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
