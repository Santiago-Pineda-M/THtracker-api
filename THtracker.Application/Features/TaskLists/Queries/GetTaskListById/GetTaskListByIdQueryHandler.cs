using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.TaskLists.Queries.GetTaskListById;

public sealed class GetTaskListByIdQueryHandler : IRequestHandler<GetTaskListByIdQuery, Result<TaskListResponse>>
{
    private readonly ITaskListRepository _taskListRepository;

    public GetTaskListByIdQueryHandler(ITaskListRepository taskListRepository)
    {
        _taskListRepository = taskListRepository;
    }

    public async Task<Result<TaskListResponse>> Handle(GetTaskListByIdQuery request, CancellationToken cancellationToken)
    {
        var taskList = await _taskListRepository.GetByIdAsync(request.Id, cancellationToken);

        if (taskList == null || taskList.UserId != request.UserId)
        {
            return Result.Failure<TaskListResponse>(new Error("NotFound", "La lista de tareas no existe o no tienes acceso."));
        }

        return Result.Success(new TaskListResponse(
            taskList.Id,
            taskList.UserId,
            taskList.Name,
            taskList.Color,
            taskList.CreatedAt,
            taskList.UpdatedAt
        ));
    }
}
