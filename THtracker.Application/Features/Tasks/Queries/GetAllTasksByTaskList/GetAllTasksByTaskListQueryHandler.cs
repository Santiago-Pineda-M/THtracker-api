using MediatR;
using THtracker.Application.Common;
using THtracker.Application.Features.Tasks;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.Tasks.Queries.GetAllTasksByTaskList;

public sealed class GetAllTasksByTaskListQueryHandler : IRequestHandler<GetAllTasksByTaskListQuery, Result<PaginatedResponse<TaskResponse>>>
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskListRepository _taskListRepository;

    public GetAllTasksByTaskListQueryHandler(ITaskRepository taskRepository, ITaskListRepository taskListRepository)
    {
        _taskRepository = taskRepository;
        _taskListRepository = taskListRepository;
    }

    public async Task<Result<PaginatedResponse<TaskResponse>>> Handle(
        GetAllTasksByTaskListQuery request,
        CancellationToken cancellationToken)
    {
        var taskList = await _taskListRepository.GetByIdAsync(request.TaskListId, cancellationToken);

        if (taskList == null || taskList.UserId != request.UserId)
        {
            return Result.Failure<PaginatedResponse<TaskResponse>>(
                new Error("NotFound", "La lista de tareas no existe o no tienes acceso."));
        }

        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _taskRepository.GetPageByTaskListAsync(
            request.TaskListId,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = page.Items
            .Select(t => new TaskResponse(
                t.Id,
                t.TaskListId,
                t.UserId,
                t.Content,
                t.IsCompleted,
                t.DueDate,
                t.CreatedAt,
                t.UpdatedAt))
            .ToList();

        return Result.Success(new PaginatedResponse<TaskResponse>(
            items,
            page.TotalCount,
            pageNumber,
            pageSize));
    }
}
