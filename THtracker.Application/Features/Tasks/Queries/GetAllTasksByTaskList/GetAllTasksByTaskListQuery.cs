using MediatR;
using THtracker.Application.Common;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Tasks.Queries.GetAllTasksByTaskList;

public sealed record GetAllTasksByTaskListQuery(
    Guid TaskListId,
    Guid UserId,
    int PageNumber = 1,
    int PageSize = Pagination.DefaultPageSize) : IRequest<Result<PaginatedResponse<TaskResponse>>>;
