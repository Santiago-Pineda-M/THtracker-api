using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Entities;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Tasks.Commands.CreateTask;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskListRepository _taskListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(
        ITaskRepository taskRepository,
        ITaskListRepository taskListRepository,
        IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _taskListRepository = taskListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var taskList = await _taskListRepository.GetByIdAsync(request.TaskListId, cancellationToken);

        if (taskList == null || taskList.UserId != request.UserId)
        {
            return Result.Failure<TaskResponse>(new Error("NotFound", "La lista de tareas no existe o no tienes acceso."));
        }

        var task = new TaskItem(request.TaskListId, request.UserId, request.Content, request.DueDate);

        await _taskRepository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TaskResponse(
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
