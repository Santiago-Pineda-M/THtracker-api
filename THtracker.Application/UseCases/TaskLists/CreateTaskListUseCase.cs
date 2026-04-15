namespace THtracker.Application.UseCases.TaskLists;

using FluentValidation;
using THtracker.Application.DTOs.TaskLists;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

public class CreateTaskListUseCase
{
    private readonly ITaskListRepository taskListRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly IValidator<CreateTaskListRequest> validator;

    public CreateTaskListUseCase(
        ITaskListRepository taskListRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateTaskListRequest> validator
    )
    {
        this.taskListRepository = taskListRepository;
        this.unitOfWork = unitOfWork;
        this.validator = validator;
    }

    public async Task<Result<TaskListResponse>> ExecuteAsync(
        Guid userId,
        CreateTaskListRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var validationResult = await this.validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(" | ", validationResult.Errors.Select(e => e.ErrorMessage));
            return Result.Failure<TaskListResponse>(new Error("Validation", errors));
        }

        var taskList = new TaskList(userId, request.Name, request.Color);

        await this.taskListRepository.AddAsync(taskList, cancellationToken);
        await this.unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaskListResponse(taskList.Id, taskList.UserId, taskList.Name, taskList.Color, taskList.CreatedAt, taskList.UpdatedAt);
    }
}
