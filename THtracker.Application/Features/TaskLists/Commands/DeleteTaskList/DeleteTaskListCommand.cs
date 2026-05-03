using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.TaskLists.Commands.DeleteTaskList;

public sealed record DeleteTaskListCommand(Guid Id, Guid UserId) : IRequest<Result>;
