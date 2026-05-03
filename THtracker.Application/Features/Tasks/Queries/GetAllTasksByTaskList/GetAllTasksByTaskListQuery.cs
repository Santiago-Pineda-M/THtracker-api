using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Tasks.Queries.GetAllTasksByTaskList;

public sealed record GetAllTasksByTaskListQuery(Guid TaskListId, Guid UserId) : IRequest<Result<IEnumerable<TaskResponse>>>;
