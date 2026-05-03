using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Tasks.Commands.DeleteTask;

public sealed record DeleteTaskCommand(Guid Id, Guid UserId) : IRequest<Result<Unit>>;
