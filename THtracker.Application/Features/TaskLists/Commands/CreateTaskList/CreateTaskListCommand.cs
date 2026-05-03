using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.TaskLists.Commands.CreateTaskList;

public sealed record CreateTaskListCommand(
    string Name,
    string Color,
    Guid UserId = default) : IRequest<Result<TaskListResponse>>;
