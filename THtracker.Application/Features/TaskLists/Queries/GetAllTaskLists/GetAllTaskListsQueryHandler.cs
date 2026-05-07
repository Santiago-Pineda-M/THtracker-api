using MediatR;
using THtracker.Application.Common;
using THtracker.Application.Features.TaskLists;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.TaskLists.Queries.GetAllTaskLists;

public sealed class GetAllTaskListsQueryHandler : IRequestHandler<GetAllTaskListsQuery, Result<PaginatedResponse<TaskListResponse>>>
{
    private readonly ITaskListRepository _taskListRepository;

    public GetAllTaskListsQueryHandler(ITaskListRepository taskListRepository)
    {
        _taskListRepository = taskListRepository;
    }

    public async Task<Result<PaginatedResponse<TaskListResponse>>> Handle(
        GetAllTaskListsQuery request,
        CancellationToken cancellationToken)
    {
        var (pageNumber, pageSize) = Pagination.Normalize(request.PageNumber, request.PageSize);
        var page = await _taskListRepository.GetPageByUserAsync(
            request.UserId,
            pageNumber,
            pageSize,
            cancellationToken);

        var items = page.Items
            .Select(t => new TaskListResponse(
                t.Id,
                t.UserId,
                t.Name,
                t.Color,
                t.CreatedAt,
                t.UpdatedAt))
            .ToList();

        return Result.Success(new PaginatedResponse<TaskListResponse>(
            items,
            page.TotalCount,
            pageNumber,
            pageSize));
    }
}
