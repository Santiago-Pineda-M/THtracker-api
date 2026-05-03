using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.TaskLists.Commands.UpdateTaskList;

public sealed record UpdateTaskListCommand(
    Guid Id,
    string Name,
    string Color,
    Guid UserId = default) : IRequest<Result<TaskListResponse>>;
