using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Tasks.Commands.UpdateTask;

public sealed record UpdateTaskCommand(
    Guid Id,
    string Content,
    DateTime? DueDate,
    Guid UserId = default) : IRequest<Result<TaskResponse>>;
