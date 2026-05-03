using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.TaskLists.Queries.GetTaskListById;

public sealed record GetTaskListByIdQuery(Guid Id, Guid UserId) : IRequest<Result<TaskListResponse>>;
