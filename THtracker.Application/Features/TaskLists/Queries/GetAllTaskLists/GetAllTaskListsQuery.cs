using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.TaskLists.Queries.GetAllTaskLists;

public sealed record GetAllTaskListsQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PaginatedResponse<TaskListResponse>>>;
