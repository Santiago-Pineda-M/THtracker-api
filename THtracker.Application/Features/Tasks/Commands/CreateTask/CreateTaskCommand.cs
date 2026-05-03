using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Tasks.Commands.CreateTask;

public sealed record CreateTaskCommand(
    Guid TaskListId, 
    string Content, 
    DateTime? DueDate,
    Guid UserId = default) : IRequest<Result<TaskResponse>>;
