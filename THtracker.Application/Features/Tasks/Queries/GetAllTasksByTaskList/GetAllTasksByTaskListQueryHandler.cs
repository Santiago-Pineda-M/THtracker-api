using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Tasks.Queries.GetAllTasksByTaskList;

public sealed class GetAllTasksByTaskListQueryHandler : IRequestHandler<GetAllTasksByTaskListQuery, Result<IEnumerable<TaskResponse>>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskListRepository _taskListRepository;

    public GetAllTasksByTaskListQueryHandler(ITaskRepository taskRepository, ITaskListRepository taskListRepository)
    {
        _taskRepository = taskRepository;
        _taskListRepository = taskListRepository;
    }

    public async Task<Result<IEnumerable<TaskResponse>>> Handle(GetAllTasksByTaskListQuery request, CancellationToken cancellationToken)
    {
        var taskList = await _taskListRepository.GetByIdAsync(request.TaskListId, cancellationToken);

        if (taskList == null || taskList.UserId != request.UserId)
        {
            return Result.Failure<IEnumerable<TaskResponse>>(new Error("NotFound", "La lista de tareas no existe o no tienes acceso."));
        }

        var tasks = await _taskRepository.GetAllByTaskListAsync(request.TaskListId, cancellationToken);
        
        var response = tasks.Select(t => new TaskResponse(
            t.Id,
            t.TaskListId,
            t.UserId,
            t.Content,
            t.IsCompleted,
            t.DueDate,
            t.CreatedAt,
            t.UpdatedAt
        ));

        return Result.Success(response);
    }
}
