using MediatR;
using THtracker.Domain.Common;

namespace THtracker.Application.Features.Tasks.Queries.GetTaskById;

public sealed record GetTaskByIdQuery(Guid Id, Guid UserId) : IRequest<Result<TaskResponse>>;
