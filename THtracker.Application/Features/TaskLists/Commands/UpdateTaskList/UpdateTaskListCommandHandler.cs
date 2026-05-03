using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.TaskLists.Commands.UpdateTaskList;

public sealed class UpdateTaskListCommandHandler : IRequestHandler<UpdateTaskListCommand, Result<TaskListResponse>>
{
    private readonly ITaskListRepository _taskListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskListCommandHandler(ITaskListRepository taskListRepository, IUnitOfWork unitOfWork)
    {
        _taskListRepository = taskListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TaskListResponse>> Handle(UpdateTaskListCommand request, CancellationToken cancellationToken)
    {
        var taskList = await _taskListRepository.GetByIdAsync(request.Id, cancellationToken);

        if (taskList == null || taskList.UserId != request.UserId)
        {
            return Result.Failure<TaskListResponse>(new Error("NotFound", "La lista de tareas no existe o no tienes acceso."));
        }

        taskList.Update(request.Name, request.Color);
        await _taskListRepository.UpdateAsync(taskList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
