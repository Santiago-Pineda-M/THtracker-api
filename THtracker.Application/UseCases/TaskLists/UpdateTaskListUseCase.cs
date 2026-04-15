namespace THtracker.Application.UseCases.TaskLists;

using FluentValidation;
using THtracker.Application.DTOs.TaskLists;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

public class UpdateTaskListUseCase
{
    private readonly ITaskListRepository taskListRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IValidator<UpdateTaskListRequest> validator;

    public UpdateTaskListUseCase(
        ITaskListRepository taskListRepository,
        IUnitOfWork unitOfWork,
        IValidator<UpdateTaskListRequest> validator
    )
    {
        this.taskListRepository = taskListRepository;
        this.unitOfWork = unitOfWork;
        this.validator = validator;
    }

    public async Task<Result<TaskListResponse>> ExecuteAsync(
        Guid userId,
        Guid id,
        UpdateTaskListRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await this.validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<TaskListResponse>(new Error("Validation", errors));
        }

        var taskList = await this.taskListRepository.GetByIdAsync(id, cancellationToken);

        if (taskList == null)
        {
            return Result.Failure<TaskListResponse>(new Error("NotFound", "Task list not found"));
        }

        if (taskList.UserId != userId)
        {
            return Result.Failure<TaskListResponse>(new Error("Forbidden", "You do not own this task list"));
        }

        taskList.Update(request.Name, request.Color);
        await this.taskListRepository.UpdateAsync(taskList, cancellationToken);
        await this.unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaskListResponse(
            taskList.Id,
            taskList.UserId,
            taskList.Name,
            taskList.Color,
            taskList.CreatedAt,
            taskList.UpdatedAt
        );
    }
}
