using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.TaskLists.Queries.GetAllTaskLists;

public sealed record GetAllTaskListsQuery(Guid UserId) : IRequest<Result<IEnumerable<TaskListResponse>>>;
