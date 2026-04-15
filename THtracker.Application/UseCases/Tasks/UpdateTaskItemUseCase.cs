namespace THtracker.Application.UseCases.Tasks;

using FluentValidation;
using THtracker.Application.DTOs.Tasks;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

public class UpdateTaskItemUseCase
{
    private readonly ITaskRepository taskRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IValidator<UpdateTaskItemRequest> validator;

    public UpdateTaskItemUseCase(
        ITaskRepository taskRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateTaskItemRequest> validator
    )
    {
        this.taskRepository = taskRepository;
        this.unitOfWork = unitOfWork;
        this.validator = validator;
    }

    public async Task<Result<TaskItemResponse>> ExecuteAsync(
        Guid userId,
        Guid id,
        UpdateTaskItemRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await this.validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<TaskItemResponse>(new Error("Validation", errors));
        }

        var task = await this.taskRepository.GetByIdAsync(id, cancellationToken);

        if (task == null)
        {
            return Result.Failure<TaskItemResponse>(new Error("NotFound", "Task not found"));
        }

        if (task.UserId != userId)
        {
            return Result.Failure<TaskItemResponse>(new Error("Forbidden", "You do not own this task"));
        }

        task.UpdateContent(request.Content, request.DueDate);
        await this.taskRepository.UpdateAsync(task, cancellationToken);
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
