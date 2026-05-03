using MediatR;
using THtracker.Domain.Common;
using THtracker.Domain.Interfaces;

namespace THtracker.Application.Features.TaskLists.Queries.GetAllTaskLists;

public sealed class GetAllTaskListsQueryHandler : IRequestHandler<GetAllTaskListsQuery, Result<IEnumerable<TaskListResponse>>>
{
    private readonly ITaskListRepository _taskListRepository;

    public GetAllTaskListsQueryHandler(ITaskListRepository taskListRepository)
    {
        _taskListRepository = taskListRepository;
    }

    public async Task<Result<IEnumerable<TaskListResponse>>> Handle(GetAllTaskListsQuery request, CancellationToken cancellationToken)
    {
        var taskLists = await _taskListRepository.GetAllByUserAsync(request.UserId, cancellationToken);
        
        var response = taskLists.Select(t => new TaskListResponse(
            t.Id,
            t.UserId,
            t.Name,
            t.Color,
            t.CreatedAt,
            t.UpdatedAt
        ));

        return Result.Success(response);
    }
}
